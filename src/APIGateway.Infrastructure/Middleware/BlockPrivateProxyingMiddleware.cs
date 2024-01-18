using Microsoft.AspNetCore.Http;
using TGF.CA.Infrastructure.Middleware.Security;

namespace APIGateway.Infrastructure.Middleware
{
    /// <summary>
    /// Middleware to block requests that contain "/private/" in the URL.
    /// </summary>
    public class BlockPrivateProxyingMiddleware : BlockPrivateProxyingMiddlewareBase
    {
        public BlockPrivateProxyingMiddleware(RequestDelegate next)
            : base(PrivateEndpointPaths.Default, next)
        {
        }
    }

}
