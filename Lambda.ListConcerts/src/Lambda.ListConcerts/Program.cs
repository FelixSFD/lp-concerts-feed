using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Database.ConcertBookmarks;
using Database.Concerts;
using Lambda.Common.ApiGateway;
using Lambda.ListConcerts;

await LambdaBootstrapBuilder.Create((Func<APIGatewayProxyRequest, ILambdaContext, Task<APIGatewayProxyResponse>>)Handler, new SourceGeneratorLambdaJsonSerializer<ApiGatewayJsonContext>(options => {
        options.PropertyNameCaseInsensitive = true;
    }))
    .Build()
    .RunAsync();
return;

// calls the function
async Task<APIGatewayProxyResponse> Handler(APIGatewayProxyRequest input, ILambdaContext ctx) => await new Function(DynamoDbConcertRepository.CreateDefault(ctx.Logger), DynamoDbConcertBookmarkRepository.CreateDefault(ctx.Logger)).FunctionHandler(input, ctx);