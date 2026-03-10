using System.Threading.RateLimiting;

namespace WsdlViewer.Middleware;

public static class RateLimiting
{
    public const string PolicyName = "RenderingRateLimitPolicy";

    public static void AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddPolicy(PolicyName, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 10,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1)
                    })
                );

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = (context, _) =>
            {
                context.HttpContext.Response.Redirect("/TooManyRequests");
                return ValueTask.CompletedTask;
            };
        });
    }
}
