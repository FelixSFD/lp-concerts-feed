using System.Diagnostics;
using Amazon.Lambda.Core;
using Database.Setlists;
using Microsoft.EntityFrameworkCore;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.SetlistsDbMigration;

public class Function
{
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task<string> FunctionHandler(ILambdaContext context)
    {
        var connectionString = Environment.GetEnvironmentVariable("SETLISTS_DB_CONNECTION_STRING") ?? throw new Exception("Missing environment variable 'SETLISTS_DB_CONNECTION_STRING'!");
        var stopwatch = Stopwatch.StartNew();
        var optBuilder = new DbContextOptionsBuilder<SetlistsDbContext>();
        optBuilder.UseMySQL(connectionString);
        var dbContext = new SetlistsDbContext(optBuilder.Options);
        stopwatch.Stop();
        context.Logger.LogDebug("Init duration of EFCore context: {duration} ms", stopwatch.ElapsedMilliseconds);
        context.Logger.LogInformation("Migrating database...");
        stopwatch.Restart();
        await dbContext.Database.MigrateAsync();
        stopwatch.Stop();
        context.Logger.LogInformation("EFCore migration took {duration} ms", stopwatch.ElapsedMilliseconds);

        return $"Done in {stopwatch.ElapsedMilliseconds} ms";
    }
}