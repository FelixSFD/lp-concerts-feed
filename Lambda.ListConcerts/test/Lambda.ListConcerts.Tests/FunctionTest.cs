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
                "2025-02-21T00:00:00.000Z",
                "2025-05-01T00:00:00.000Z",
                (new DateTimeOffset(2025, 2, 21, 0, 0, 0, TimeSpan.Zero),
                    new DateTimeOffset(2025, 5, 1, 0, 0, 0, TimeSpan.Zero))
            },
            {
                "2025-02-21T01:00:00.000+01:00",
                "2025-05-01T02:00:00.000+02:00",
                (new DateTimeOffset(2025, 2, 21, 1, 0, 0, TimeSpan.FromHours(1)),
                    new DateTimeOffset(2025, 5, 1, 2, 0, 0, TimeSpan.FromHours(2)))
            },
            {
                "2025-01-11T11:00:00.000+00:00",
                "2025-07-09T13:00:00.000+00:15",
                (new DateTimeOffset(2025, 1, 11, 11, 0, 0, TimeSpan.Zero),
                    new DateTimeOffset(2025, 7, 9, 13, 0, 0, TimeSpan.FromMinutes(15)))
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
        
        // we always want the UTC time!
        if (result?.from != null)
            Assert.Equal(TimeSpan.Zero, result?.from?.Offset);
        
        if (result?.to != null)
            Assert.Equal(TimeSpan.Zero, result?.to?.Offset);
    }
}