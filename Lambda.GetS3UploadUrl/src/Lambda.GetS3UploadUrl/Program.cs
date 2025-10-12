using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Lambda.Common.ApiGateway;
using Lambda.GetS3UploadUrl;

await LambdaBootstrapBuilder.Create((Func<APIGatewayProxyRequest, ILambdaContext, Task<APIGatewayProxyResponse>>)Handler, new SourceGeneratorLambdaJsonSerializer<ApiGatewayJsonContext>(options => {
        options.PropertyNameCaseInsensitive = true;
    }))
    .Build()
    .RunAsync();
return;

// calls the function
async Task<APIGatewayProxyResponse> Handler(APIGatewayProxyRequest input, ILambdaContext ctx) => await new Function().FunctionHandler(input, ctx);