using Amazon.Lambda.CloudWatchEvents.ScheduledEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.SQSEvents;
using Lambda.ConcertReminder;

await LambdaBootstrapBuilder.Create((Func<ScheduledEvent, ILambdaContext, Task>)Handler, new SourceGeneratorLambdaJsonSerializer<LocalJsonSerializer>())
    .Build()
    .RunAsync();
return;

// calls the function
async Task Handler(ScheduledEvent input, ILambdaContext ctx) => await new Function().FunctionHandler(input, ctx);