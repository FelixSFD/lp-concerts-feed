using Database.Tours;
using Database.Tours.Repositories;
using Microsoft.EntityFrameworkCore;
using Service.Tours;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddScoped<LocationService>();

builder.Services.AddControllers();

var app = builder.Build();

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