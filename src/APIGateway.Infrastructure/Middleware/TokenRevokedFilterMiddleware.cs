using APIGateway.Application;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;


namespace APIGateway.Infrastructure.Middleware
{
    public class TokenFilterMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenFilterMiddleware(RequestDelegate aNext)
        {
            _next = aNext;
        }

        /// <summary>
        /// Invokes the middleware to check if the request's authentication token has been black-listed or not.
        /// If it has been black-listed, the request is blocked and 401 Unauthorized is replied.
        /// Otherwise, the request is passed to the next middleware.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext aContext)
        {
            var lTokenRevocationService = aContext.RequestServices.GetRequiredService<ITokenRevocationService>();

            // Extract JWT from the request header
            var lJwtToken = aContext.Request.Headers["Authorization"].ToString().Split(' ').LastOrDefault();

            // If there's a JWT and the JWT is revoked, then return a 401 Unauthorized response with revoked message.
            // Otherwise, continue processing.
            if (!string.IsNullOrEmpty(lJwtToken) && lTokenRevocationService.IsAccessTokenRevoked(lJwtToken))
            {
                aContext.Response.StatusCode = 401;
                await aContext.Response.WriteAsync("This token has been revoked.");
                return;
            }

            await _next(aContext);
        }
    }

}
