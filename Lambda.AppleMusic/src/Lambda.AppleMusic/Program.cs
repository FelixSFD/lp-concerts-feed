/*using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Lambda.Common.ApiGateway;
using Lambda.AppleMusic;

await LambdaBootstrapBuilder.Create((Func<APIGatewayProxyRequest, ILambdaContext, APIGatewayProxyResponse>)Handler, new SourceGeneratorLambdaJsonSerializer<ApiGatewayJsonContext>(options => {
        options.PropertyNameCaseInsensitive = true;
    }))
    .Build()
    .RunAsync();
return;

// calls the function
APIGatewayProxyResponse Handler(APIGatewayProxyRequest input, ILambdaContext ctx) => new Function().FunctionHandler(input, ctx);
*/