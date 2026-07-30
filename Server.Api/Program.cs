using Database.Tours;
using Database.Tours.Repositories;
using Microsoft.EntityFrameworkCore;
using Server.Api.ExceptionHandling;
using Service.Tours;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi("v3");
builder.Services.AddOpenApi(opt =>
{
    opt.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Version = "3.0.0";
        document.Info.Title = "LPshows.live API v3";
        document.Info.Description = "This is the API for the Linkin Park Concert Calendar fan project";
        return Task.CompletedTask;
    });
});

var connectionString = builder.Configuration.GetConnectionString("lpdb") ??
                          throw new Exception("Connection string 'lpdb' missing!");

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

builder.Services.AddControllers();

//Register Problem Details Service for API Errors
builder.Services.AddProblemDetails();

//Register the GlobalExceptionHandler
//Custom Global Exception Handler for HTTP Status Codes
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

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

app.MapControllers();

app.UseHttpsRedirection();


app.Run();