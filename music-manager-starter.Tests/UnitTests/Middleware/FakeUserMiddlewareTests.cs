using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using music_manager_starter.Server.Middleware;
using Xunit;

namespace music_manager_starter.Tests.UnitTests.Middleware
{
    public class FakeUserMiddlewareTests
    {
        [Fact]
        public async Task InvokeAsync_WithCustomHeader_UsesHeaderValue()
        {
            // Arrange
            var customUserId = "custom-user-456";
            var context = new DefaultHttpContext();
            context.Request.Headers["X-Test-User"] = customUserId;

            var wasCalled = false;
            RequestDelegate next = (ctx) =>
            {
                wasCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new FakeUserMiddleware(next);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.True(wasCalled);
            Assert.True(context.User.Identity?.IsAuthenticated ?? false);
            Assert.Equal(customUserId, context.User.FindFirstValue(ClaimTypes.NameIdentifier));
            Assert.Equal(customUserId, context.User.FindFirstValue(ClaimTypes.Name));
        }

        [Fact]
        public async Task InvokeAsync_WithoutHeader_UsesDefaultUserId()
        {
            // Arrange
            var context = new DefaultHttpContext();

            var wasCalled = false;
            RequestDelegate next = (ctx) =>
            {
                wasCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new FakeUserMiddleware(next);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.True(wasCalled);
            Assert.True(context.User.Identity?.IsAuthenticated ?? false);
            Assert.Equal("test-user-001", context.User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        [Fact]
        public async Task InvokeAsync_WithEmptyHeader_UsesDefaultUserId()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["X-Test-User"] = ""; // Empty string

            var wasCalled = false;
            RequestDelegate next = (ctx) =>
            {
                wasCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new FakeUserMiddleware(next);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.True(wasCalled);
            Assert.True(context.User.Identity?.IsAuthenticated ?? false);
            Assert.Equal("test-user-001", context.User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        [Fact]
        public async Task InvokeAsync_PreservesOtherHeaders()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["X-Test-User"] = "test-user-123";
            context.Request.Headers["X-Other-Header"] = "some-value";

            string? capturedHeader1 = null;
            string? capturedHeader2 = null;
            RequestDelegate next = (ctx) =>
            {
                capturedHeader1 = ctx.Request.Headers["X-Test-User"];
                capturedHeader2 = ctx.Request.Headers["X-Other-Header"];
                return Task.CompletedTask;
            };

            var middleware = new FakeUserMiddleware(next);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal("test-user-123", capturedHeader1);
            Assert.Equal("some-value", capturedHeader2);
        }
    }
}