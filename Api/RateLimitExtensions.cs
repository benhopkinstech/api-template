using System.Threading.RateLimiting;

namespace Api
{
    public static class RateLimitExtensions
    {
        public static IServiceCollection AddRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        context.HttpContext.Response.Headers.Add("X-Retry-After", retryAfter.ToString());
                    }

                    await context.HttpContext.Response.WriteAsync("", cancellationToken: token);
                };
                options.AddPolicy("fw2req5m", httpContext => GetFixedWindowLimiterByEndpointAndIp(httpContext, 2, TimeSpan.FromMinutes(5)));
                options.AddPolicy("fw5req5m", httpContext => GetFixedWindowLimiterByEndpointAndIp(httpContext, 5, TimeSpan.FromMinutes(5)));
                options.AddPolicy("fw10req5m", httpContext => GetFixedWindowLimiterByEndpointAndIp(httpContext, 10, TimeSpan.FromMinutes(5)));
                options.AddPolicy("fw2req10m", httpContext => GetFixedWindowLimiterByEndpointAndIp(httpContext, 2, TimeSpan.FromMinutes(10)));
                options.AddPolicy("fw5req10m", httpContext => GetFixedWindowLimiterByEndpointAndIp(httpContext, 5, TimeSpan.FromMinutes(10)));
            });

            return services;
        }

        public static WebApplication UseRateLimting(this WebApplication app)
        {
            app.UseForwardedHeaders();
            app.UseRateLimiter();

            return app;
        }

        private static RateLimitPartition<string> GetFixedWindowLimiterByEndpointAndIp(HttpContext httpContext, int permitLimit, TimeSpan window)
        {
            var endpoint = httpContext.Request.Path.ToString();
            var ip = httpContext.Connection.RemoteIpAddress;

            if (endpoint == null || ip == null)
            {
                return RateLimitPartition.GetNoLimiter("");
            }
            else
            {
                return RateLimitPartition.GetFixedWindowLimiter(
                    $"{endpoint}:{ip}",
                    factory =>
                    {
                        return new FixedWindowRateLimiterOptions
                        {
                            Window = window,
                            PermitLimit = permitLimit,
                        };
                    });
            }
        }
    }
}
