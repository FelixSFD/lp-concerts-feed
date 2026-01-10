using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.SQSEvents;
using Lambda.Push.Sender;

await LambdaBootstrapBuilder.Create((Func<SQSEvent, ILambdaContext, Task>)Handler, new SourceGeneratorLambdaJsonSerializer<SqsEventJsonSerializer>())
    .Build()
    .RunAsync();
return;

// calls the function
async Task Handler(SQSEvent input, ILambdaContext ctx) => await new Function().FunctionHandler(input, ctx);