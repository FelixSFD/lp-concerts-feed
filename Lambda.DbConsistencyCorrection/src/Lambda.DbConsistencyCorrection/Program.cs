using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Lambda.DbConsistencyCorrection;

await LambdaBootstrapBuilder.Create(Handler)
    .Build()
    .RunAsync();
return;

// calls the function
async Task Handler(ILambdaContext ctx) => await new Function(ctx).FunctionHandler(ctx);