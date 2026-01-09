using System.Security.Claims;

namespace music_manager_starter.Server.Middleware
{
    /// <summary>
    /// Injecting a fake authenticated user with middleware.
    /// In a real coding environment, I would either work on the auth system first, or mock it out if I am waiting on another developer to finish.
    /// </summary>
    public sealed class FakeUserMiddleware
    {
        private readonly RequestDelegate _next;

        public FakeUserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var userId =
                context.Request.Headers.TryGetValue("X-Test-User", out var headerUser)
                    ? headerUser.ToString()
                    : "test-user-001";

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
