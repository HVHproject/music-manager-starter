using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using music_manager_starter.Data;
using music_manager_starter.Data.Models;
using music_manager_starter.Server;
using music_manager_starter.Shared;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace music_manager_starter.Tests.IntegrationTests
{
    public class RatingsControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public RatingsControllerTests()
        {
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Test");
                    builder.ConfigureServices(services =>
                    {
                        // Remove the existing DbContext registration
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<DataDbContext>));

                        if (descriptor != null)
                        {
                            services.Remove(descriptor);
                        }

                        // Add InMemory database for testing
                        services.AddDbContext<DataDbContext>(options =>
                        {
                            options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid().ToString());
                        });
                    });
                });

            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task PostRating_WithValidData_ReturnsOk()
        {
            // Arrange
            var songId = Guid.NewGuid();
            var ratingValue = 4.5;

            // Set up test user header
            _client.DefaultRequestHeaders.Add("X-Test-User", "test-user-123");

            // Create a song in the database first
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DataDbContext>();
                dbContext.Songs.Add(new Data.Models.Song
                {
                    Id = songId,
                    Title = "Test Song",
                    Artist = "Test Artist"
                });
                await dbContext.SaveChangesAsync();
            }

            // Act
            var response = await _client.PostAsJsonAsync(
                $"/api/ratings/{songId}",
                ratingValue
            );

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Verify the rating was created
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DataDbContext>();
                var rating = await dbContext.Ratings
                    .FirstOrDefaultAsync(r => r.SongId == songId);

                Assert.NotNull(rating);
                Assert.Equal((decimal)ratingValue, rating.Value);
                Assert.Equal("test-user-123", rating.UserId);
            }
        }

        [Fact]
        public async Task PostRating_WithoutUserHeader_UsesDefaultUserId()
        {
            // Arrange
            var songId = Guid.NewGuid();
            var ratingValue = 3.0;

            // Don't set X-Test-User header

            // Create a song
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DataDbContext>();
                dbContext.Songs.Add(new Data.Models.Song { Id = songId, Title = "Test Song" });
                await dbContext.SaveChangesAsync();
            }

            // Act
            var response = await _client.PostAsJsonAsync(
                $"/api/ratings/{songId}",
                ratingValue
            );

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Verify default user ID was used
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DataDbContext>();
                var rating = await dbContext.Ratings
                    .FirstOrDefaultAsync(r => r.SongId == songId);

                Assert.NotNull(rating);
                Assert.Equal("test-user-001", rating.UserId);
            }
        }

        [Fact]
        public async Task GetSummary_ReturnsCorrectData()
        {
            // Arrange
            var songId = Guid.NewGuid();
            var userId = "test-user-123";

            _client.DefaultRequestHeaders.Add("X-Test-User", userId);

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DataDbContext>();

                dbContext.Songs.Add(new Data.Models.Song { Id = songId, Title = "Test Song" });

                // Add ratings
                dbContext.Ratings.AddRange(
                    new Rating { SongId = songId, UserId = userId, Value = 4.0m },
                    new Rating { SongId = songId, UserId = "user-002", Value = 5.0m },
                    new Rating { SongId = songId, UserId = "user-003", Value = 3.0m }
                );

                await dbContext.SaveChangesAsync();
            }

            // Act
            var response = await _client.GetAsync($"/api/ratings/{songId}");
            var summary = await response.Content.ReadFromJsonAsync<RatingSummaryDto>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(summary);
            Assert.Equal(songId, summary.SongId);
            Assert.Equal(4.0, summary.UserRating); // Current user's rating
            Assert.Equal(3, summary.TotalRatings);
            Assert.Equal(4.0, summary.Average); // (4 + 5 + 3) / 3 = 4.0
            Assert.Contains(summary.Distribution, count => count > 0);
        }
    }
}