using Amazon.Lambda.Core;
using Database.Concerts;
using LPCalendar.DataStructure;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.DbConsistencyCorrection;

public class Function
{
    private readonly IConcertRepository _concertRepository;
    private readonly ILambdaLogger _logger;

    public Function(ILambdaContext context)
    {
        _concertRepository = DynamoDbConcertRepository.CreateDefault(context.Logger);
        _logger = context.Logger;
    }


    public async Task FunctionHandler(ILambdaContext context)
    {
        _logger.LogInformation("Running consistency correction for concerts...");
        await _concertRepository.GetConcertsAsync(DateTimeOffset.MinValue)
            .ForEachAwaitAsync(FixConcert);
        _logger.LogInformation("Consistency correction for concerts complete.");
    }

    private async Task FixConcert(Concert concert)
    {
        var didChange = false;
        if (concert.LastChange == null)
        {
            _logger.LogInformation("Concert '{id}' is missing the field 'LastChange'. Will set default value.", concert.Id);
            concert.LastChange = DateTimeOffset.UnixEpoch;
            didChange = true;
        }

        if (didChange)
        {
            _logger.LogInformation("Concert '{id}' has pending corrections. Will update it in DB.", concert.Id);
            await _concertRepository.SaveAsync(concert);
            _logger.LogInformation("Saved Concert '{id}' has pending corrections. Will update it in DB.", concert.Id);
        }
    }
}