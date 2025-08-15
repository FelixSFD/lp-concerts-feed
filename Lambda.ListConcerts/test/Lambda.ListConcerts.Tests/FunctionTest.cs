using System.Reflection;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Xunit;

namespace Lambda.ListConcerts.Tests;

using DateRangeTuple = (DateTimeOffset? from, DateTimeOffset? to);

public class FunctionTest
{
    private static ILambdaLogger CreateLambdaLogger()
    {
        return new TestLambdaLogger();
    }


    public static TheoryData<string?, string?, DateRangeTuple> GetDateRange_Data()
    {
        return new TheoryData<string?, string?, DateRangeTuple>
        {
            {
                null,
                null,
                new DateRangeTuple(null, null)
            },
            {
                "2025-02-21T00:00:00.00Z",
                "2025-05-01T00:00:00.00Z",
                (new DateTimeOffset(2025, 2, 21, 0, 0, 0, TimeSpan.Zero),
                    new DateTimeOffset(2025, 5, 1, 0, 0, 0, TimeSpan.Zero))
            }
        };
    }

    [Theory]
    [MemberData(nameof(GetDateRange_Data))]
    public void GetDateRangeFrom(string? from, string? to, DateRangeTuple expectedResult)
    {
        var lambdaLogger = CreateLambdaLogger();
        var methodInfo = typeof(Function).GetMethod("GetDateRangeFrom", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(methodInfo);
        
        var result = (DateRangeTuple?)methodInfo.Invoke(null, [from, to, lambdaLogger]);
        Assert.NotNull(result);
        Assert.Equal(expectedResult, result);
    }
}