using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using music_manager_starter.Server.Controllers;
using music_manager_starter.Server.Services;
using music_manager_starter.Shared;
using FluentAssertions;
using Xunit;

namespace music_manager_starter.Tests.IntegrationTests
{
    public class RatingsControllerTests
    {
        private readonly Mock<IRatingService> _mockRatingService;
        private readonly RatingsController _controller;

        public RatingsControllerTests()
        {
            _mockRatingService = new Mock<IRatingService>();
            _controller = new RatingsController(_mockRatingService.Object);

            // Setup fake user for controller context
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-123"),
                new Claim(ClaimTypes.Name, "test-user-123")
            }, "Test"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task Rate_ValidRating_CallsService()
        {
            // Arrange
            var songId = Guid.NewGuid();
            var ratingValue = 4.5;

            _mockRatingService.Setup(s => s.RateSongAsync(
                songId,
                ratingValue,
                "test-user-123"))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await _controller.Rate(songId, ratingValue);

            // Assert using FluentAssertions
            result.Should().BeOfType<OkResult>();
            _mockRatingService.Verify();
        }

        [Fact]
        public async Task Summary_ValidRequest_ReturnsServiceResult()
        {
            // Arrange
            var songId = Guid.NewGuid();
            var expectedSummary = new RatingSummaryDto
            {
                SongId = songId,
                Average = 4.5,
                TotalRatings = 10,
                UserRating = 5.0,
                Distribution = new[] { 0, 0, 1, 2, 3, 4, 0, 0, 0, 0, 0 }
            };

            _mockRatingService.Setup(s => s.GetSummaryAsync(
                songId,
                "test-user-123"))
                .ReturnsAsync(expectedSummary);

            // Act
            var result = await _controller.Summary(songId);

            // Assert using FluentAssertions
            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(expectedSummary);
        }

        [Fact]
        public async Task Rate_InvalidRating_ReturnsBadRequest()
        {
            // Arrange
            var songId = Guid.NewGuid();
            var invalidRating = 6.0; // Out of range

            _mockRatingService.Setup(s => s.RateSongAsync(
                It.IsAny<Guid>(),
                It.IsAny<double>(),
                It.IsAny<string>()))
                .ThrowsAsync(new ArgumentOutOfRangeException());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => _controller.Rate(songId, invalidRating));
        }
    }
}