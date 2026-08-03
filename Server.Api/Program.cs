using Common.Utils.Cache;
using Database.Tours;
using Database.Tours.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Server.Api.Cache;
using Server.Api.ExceptionHandling;
using Server.Api.HealthChecks;
using Service.Tours;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables("App_");

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy
                .WithOrigins(builder.Configuration.GetValue<string[]>("CORS:AllowedOrigins") ?? ["http://localhost:4200"])
                .WithMethods(builder.Configuration.GetValue<string[]>("CORS:AllowedMethods") ?? []);
        });
});

// Configure HTTP logging
builder.Services.AddHttpLogging(opt =>
{
    opt.LoggingFields = HttpLoggingFields.All;
    opt.CombineLogs = true;
});

// Configure cache
builder.Services.AddResponseCaching(options =>
{
    options.SizeLimit = 128_000_000; // 128 MB
});
builder.Services.AddOutputCache(options =>
{
    options.SizeLimit = 128_000_000; // 128 MB
    
    // define policies
    options.AddPolicy(CachePolicyNames.Short, policy =>
    {
        policy.Cache()
            .Expire(TimeSpan.FromSeconds(CacheExpiration.Short));
    });
    options.AddPolicy(CachePolicyNames.Medium, policy =>
    {
        policy.Cache()
            .Expire(TimeSpan.FromSeconds(CacheExpiration.Medium));
    });
    options.AddPolicy(CachePolicyNames.Long, policy =>
    {
        policy.Cache()
            .Expire(TimeSpan.FromSeconds(CacheExpiration.Long));
    });
    options.AddPolicy(CachePolicyNames.VeryLong, policy =>
    {
        policy.Cache()
            .Expire(TimeSpan.FromSeconds(CacheExpiration.VeryLong));
    });
    options.AddPolicy(CachePolicyNames.BasicallyForever, policy =>
    {
        policy.Cache()
            .Expire(TimeSpan.FromSeconds(CacheExpiration.Maximum));
    });
});

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
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes?[JwtBearerDefaults.AuthenticationScheme] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme,
            BearerFormat = "JWT",
            Description = "JWT Bearer authentication"
        };
        
        return Task.CompletedTask;
    });

    opt.AddOperationTransformer((operation, context, cancellationToken) =>
    {
        var requiresAuth = context.Description.ActionDescriptor?.EndpointMetadata
            .OfType<IAuthorizeData>()
            .Any() ?? false;

        if (!requiresAuth)
            return Task.CompletedTask;
        
        // Add Security Requirement
        var bearerSchemeRef = new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, context.Document);
        operation.Security ??= new List<OpenApiSecurityRequirement>();
        operation.Security.Add(
            new OpenApiSecurityRequirement
            {
                [bearerSchemeRef] = []
            }
        );
        
        // Add additional documentation
        operation.Responses?.Add("401", new OpenApiResponse { Description = "Unauthorized: Credentials are missing or not valid" });
        operation.Responses?.Add("403", new OpenApiResponse { Description = "Forbidden: Credentials are valid, but the caller is not allowed to perform this operation" });
        
        return Task.CompletedTask;
    });
    
    opt.AddOperationTransformer((operation, context, cancellationToken) =>
    {
        // Check if it's a MapIdentityApi action
        if (context.Description.ActionDescriptor is not ControllerActionDescriptor controllerActionDescriptor)
        {
            return Task.CompletedTask;
        }

        // For other controller actions, set OperationId based on controller and action names
        operation.OperationId = $"{controllerActionDescriptor.ActionName}";
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
            },
            RoleClaimType = "cognito:groups"
        };
    });

// register API controllers
builder.Services.AddControllers(opt =>
{
    opt.CacheProfiles.Add(CachePolicyNames.Short, new CacheProfile
    {
        Duration = CacheExpiration.Short,
        Location = ResponseCacheLocation.Any,
    });
});

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
if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("EnableSwaggerUI"))
{
    app.MapGet("/openapi/v3.yaml", () => Results.File(
        Path.Combine(
            AppContext.BaseDirectory,
            "openapi_v3.yaml"),
        "text/yaml"));
    
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v3.yaml", "LPshows API");
    });
}

// enable HTTP logging
app.UseHttpLogging();

app.UseCors();

app.UseAuthentication(); // responsible for constructing AuthenticationTicket objects representing the user's identity
app.UseAuthorization();

app.UseOutputCache();
app.UseResponseCaching();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();