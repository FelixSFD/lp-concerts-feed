using Amazon.Lambda.CloudWatchEvents.ScheduledEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Database.Concerts;
using Lambda.ConcertStatusUpdater;
using LocalJsonSerializer = Lambda.ConcertStatusUpdater.LocalJsonSerializer;

await LambdaBootstrapBuilder.Create((Func<ScheduledEvent, ILambdaContext, Task>)Handler, new SourceGeneratorLambdaJsonSerializer<LocalJsonSerializer>())
    .Build()
    .RunAsync();
return;

// calls the function
async Task Handler(ScheduledEvent input, ILambdaContext ctx) => await new Function(DynamoDbConcertRepository.CreateDefault(ctx.Logger)).FunctionHandler(input, ctx);