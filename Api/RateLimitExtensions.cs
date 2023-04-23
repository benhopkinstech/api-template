using System.Threading.RateLimiting;

namespace Api
{
    public static class RateLimitExtensions
    {
        public static RateLimitPartition<string> GetFixedWindowLimiterByEndpointAndIp(HttpContext httpContext, int permitLimit, TimeSpan window)
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
