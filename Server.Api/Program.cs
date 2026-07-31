using Database.Tours;
using Database.Tours.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Server.Api.ExceptionHandling;
using Server.Api.HealthChecks;
using Service.Tours;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi("v3", opt =>
{
    opt.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Version = "3.0.0";
        document.Info.Title = "LPshows.live API v3";
        document.Info.Description = "This is the API for the Linkin Park Concert Calendar fan project";
        
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes?["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT Bearer authentication"
        };
        
        return Task.CompletedTask;
    });
});

var connectionString = builder.Configuration.GetConnectionString("lpdb") ??
                          throw new Exception("Connection string 'lpdb' missing!");

// read AWS Cognito configurations
var cognitoAppClientId = builder.Configuration["Cognito:AppClientId"];
var cognitoUserPoolId = builder.Configuration["Cognito:UserPoolId"];
var cognitoAWSRegion = builder.Configuration["Cognito:AWSRegion"];

var validIssuer = $"https://cognito-idp.{cognitoAWSRegion}.amazonaws.com/{cognitoUserPoolId}";
var validAudience = cognitoAppClientId;

builder.Services.AddDbContext<ToursDbContext>(options =>
{
    options.UseMySQL(connectionString, dbContextBuilder => dbContextBuilder.MigrationsAssembly(typeof(ToursDbContext).Assembly.FullName));
});
builder.Services.AddScoped<ICountryRepository, SqlCountryRepository>();
builder.Services.AddScoped<IStateRepository, SqlStateRepository>();
builder.Services.AddScoped<ICityRepository, SqlCityRepository>();
builder.Services.AddScoped<IVenueRepository, SqlVenueRepository>();
builder.Services.AddScoped<ITourRepository, SqlTourRepository>();
builder.Services.AddScoped<IConcertTypeRepository, SqlConcertTypeRepository>();
builder.Services.AddScoped<IConcertRepository, SqlConcertRepository>();
builder.Services.AddScoped<LocationService>();
builder.Services.AddScoped<VenueService>();
builder.Services.AddScoped<TourService>();
builder.Services.AddScoped<ConcertService>();

// Register authentication schemes, and specify the default authentication scheme
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.Authority = validIssuer;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateLifetime = true,
            // Note: Amazon Cognito returns the audience "aud" field in the ID Token, but not in the Access Token.
            // Instead, the audience is specified in the "client_id" field of the Access Token. So you'll have to manually validate the audience.
            // Second, if the AudienceValidator delegate is specified, it will be called regardless of whether ValidateAudience is set to false.
            AudienceValidator = (audiences, securityToken, validationParameters) =>
            {
                var castedToken = securityToken as JsonWebToken;
                var clientId = castedToken?.GetPayloadValue<string>("client_id");

                return validAudience == clientId;
            }
        };
    });

// register API controllers
builder.Services.AddControllers();

//Register Problem Details Service for API Errors
builder.Services.AddProblemDetails();

//Register the GlobalExceptionHandler
//Custom Global Exception Handler for HTTP Status Codes
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Configure health checks
builder.Services
    .AddHealthChecks()
    .AddCheck<DbConnectedHealthCheck>("Database Connection");

var app = builder.Build();

//Add exception handler in the middleware
app.UseExceptionHandler();

// Add Middleware so that the http status codes that do not return a JSON body will return a JSON body
app.UseStatusCodePages();

// run DB migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ToursDbContext>();
    await db.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v3.json", "LPshows API");
    });
}

app.UseAuthentication(); // responsible for constructing AuthenticationTicket objects representing the user's identity
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();