using Amazon.Lambda.Core;

namespace Lambda.SetlistsRead;

public static class LambdaContextCancellationTokenExtensions
{
    /// <summary>
    /// Returns a <see cref="CancellationToken"/> that is canceled when the Lambda timeout is reached
    /// </summary>
    /// <param name="context"></param>
    /// <returns>a new <see cref="CancellationToken"/></returns>
    public static CancellationToken GetCancellationToken(this ILambdaContext context) 
        => context.GetCancellationToken(TimeSpan.Zero);


    /// <summary>
    /// Returns a <see cref="CancellationToken"/> that is canceled when the Lambda timeout is reached
    /// </summary>
    /// <param name="context"></param>
    /// <param name="gracePeriod"><see cref="TimeSpan"/> to cancel the token earlier than the timeout. This can be used if shutting down takes quite long.</param>
    /// <returns>a new <see cref="CancellationToken"/></returns>
    public static CancellationToken GetCancellationToken(this ILambdaContext context, TimeSpan gracePeriod)
    {
        var timeout = context.RemainingTime.Subtract(gracePeriod);
        var cts = new CancellationTokenSource(timeout);
        return cts.Token;
    }
}