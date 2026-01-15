using System.Security.Claims;

namespace music_manager_starter.Server.Middleware
{
    /// <summary>
    /// Processes an HTTP request by injecting a fake user identity
    /// </summary>
    /// <param name="context">The HTTP context for the request</param>
    /// <returns>A task representing the async operation</returns>
    public sealed class FakeUserMiddleware
    {
        private readonly RequestDelegate _next;

        public FakeUserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string userId = "test-user-001";

            if (context.Request.Headers.TryGetValue("X-Test-User", out var headerUser))
            {
                var headerValue = headerUser.ToString();
                if (!string.IsNullOrWhiteSpace(headerValue))
                {
                    userId = headerValue;
                }
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, userId)
            };

            var identity = new ClaimsIdentity(claims, "Fake");
            context.User = new ClaimsPrincipal(identity);

            await _next(context);
        }
    }
}