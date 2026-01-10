using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using music_manager_starter.Shared;
using music_manager_starter.Data;
using music_manager_starter.Data.Models;
using music_manager_starter.Server.Services;
using Xunit;

namespace music_manager_starter.Tests.UnitTests.Services
{
    public class RatingServiceTests
    {
        private DataDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<DataDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new DataDbContext(options);
        }

        private RatingService CreateService(DataDbContext? context = null)
        {
            var dbContext = context ?? CreateInMemoryContext();
            return new RatingService(dbContext);
        }

        [Fact]
        public async Task RateSongAsync_ValidRating_CreatesNewRating()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var service = CreateService(context);

            var songId = Guid.NewGuid();
            var userId = "user-001";
            var ratingValue = 4.5;

            context.Songs.Add(new Data.Models.Song { Id = songId, Title = "Test Song" });
            await context.SaveChangesAsync();

            // Act
            await service.RateSongAsync(songId, ratingValue, userId);

            // Assert
            var rating = await context.Ratings
                .FirstOrDefaultAsync(r => r.SongId == songId && r.UserId == userId);

            Assert.NotNull(rating);
            Assert.Equal((decimal)ratingValue, rating.Value);
            Assert.Equal(songId, rating.SongId);
            Assert.Equal(userId, rating.UserId);
        }

        [Fact]
        public async Task RateSongAsync_ValidRating_UpdatesExistingRating()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var service = CreateService(context);

            var songId = Guid.NewGuid();
            var userId = "user-001";
            var initialRating = 3.0;
            var updatedRating = 5.0;

            context.Songs.Add(new Data.Models.Song { Id = songId, Title = "Test Song" });
            context.Ratings.Add(new Rating
            {
                SongId = songId,
                UserId = userId,
                Value = (decimal)initialRating,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            });
            await context.SaveChangesAsync();

            // Act
            await service.RateSongAsync(songId, updatedRating, userId);

            // Assert
            var rating = await context.Ratings
                .FirstOrDefaultAsync(r => r.SongId == songId && r.UserId == userId);

            Assert.NotNull(rating);
            Assert.Equal((decimal)updatedRating, rating.Value);
            Assert.True(rating.CreatedAt > DateTime.UtcNow.AddMinutes(-1));
        }

        [Theory]
        [InlineData(-0.5)]
        [InlineData(5.5)]
        [InlineData(3.7)] // Not a 0.5 increment
        [InlineData(2.3)]
        public async Task RateSongAsync_InvalidRating_ThrowsArgumentOutOfRangeException(double invalidRating)
        {
            // Arrange
            var service = CreateService();
            var songId = Guid.NewGuid();
            var userId = "user-001";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => service.RateSongAsync(songId, invalidRating, userId)
            );
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(2.5)]
        [InlineData(5.0)]
        public async Task RateSongAsync_ValidRatingValues_DoesNotThrow(double validRating)
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var service = CreateService(context);

            var songId = Guid.NewGuid();
            var userId = "user-001";

            context.Songs.Add(new Data.Models.Song { Id = songId, Title = "Test Song" });
            await context.SaveChangesAsync();

            // Act & Assert - Should not throw
            var exception = await Record.ExceptionAsync(
                () => service.RateSongAsync(songId, validRating, userId)
            );

            Assert.Null(exception);
        }

        [Fact]
        public async Task GetSummaryAsync_NoRatings_ReturnsZeroValues()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var service = CreateService(context);

            var songId = Guid.NewGuid();
            var userId = "user-001";

            context.Songs.Add(new Data.Models.Song { Id = songId, Title = "Test Song" });
            await context.SaveChangesAsync();

            // Act
            var summary = await service.GetSummaryAsync(songId, userId);

            // Assert
            Assert.Equal(songId, summary.SongId);
            Assert.Equal(0, summary.Average);
            Assert.Equal(0, summary.TotalRatings);
            Assert.Null(summary.UserRating);
            Assert.All(summary.Distribution, count => Assert.Equal(0, count));
        }

        [Fact]
        public async Task GetSummaryAsync_WithMultipleRatings_CalculatesCorrectAverage()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var service = CreateService(context);

            var songId = Guid.NewGuid();
            var userId = "user-001";

            // Seed a song
            context.Songs.Add(new Data.Models.Song { Id = songId, Title = "Test Song" });

            // Add multiple ratings
            var ratings = new[]
            {
                new Rating { SongId = songId, UserId = "user-001", Value = 4.0m },
                new Rating { SongId = songId, UserId = "user-002", Value = 5.0m },
                new Rating { SongId = songId, UserId = "user-003", Value = 3.0m },
                new Rating { SongId = songId, UserId = "user-004", Value = 4.5m }
            };
            await context.Ratings.AddRangeAsync(ratings);
            await context.SaveChangesAsync();

            // Act
            var summary = await service.GetSummaryAsync(songId, userId);

            // Assert
            Assert.Equal(4.125, summary.Average); // (4.0 + 5.0 + 3.0 + 4.5) / 4 = 4.125
            Assert.Equal(4, summary.TotalRatings);
            Assert.Equal(4.0, summary.UserRating); // User's own rating
        }

        [Fact]
        public async Task GetSummaryAsync_WithRatings_CalculatesCorrectDistribution()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var service = CreateService(context);

            var songId = Guid.NewGuid();
            var userId = "user-001";

            context.Songs.Add(new Data.Models.Song { Id = songId, Title = "Test Song" });

            // Add ratings with specific values for distribution testing
            var ratings = new[]
            {
                new Rating { SongId = songId, UserId = "user-001", Value = 5.0m },  // Index 10
                new Rating { SongId = songId, UserId = "user-002", Value = 5.0m },  // Index 10
                new Rating { SongId = songId, UserId = "user-003", Value = 4.5m },  // Index 9
                new Rating { SongId = songId, UserId = "user-004", Value = 4.0m },  // Index 8
                new Rating { SongId = songId, UserId = "user-005", Value = 2.5m }   // Index 5
            };
            await context.Ratings.AddRangeAsync(ratings);
            await context.SaveChangesAsync();

            // Act
            var summary = await service.GetSummaryAsync(songId, userId);

            // Assert
            Assert.Equal(2, summary.Distribution[10]); // Two 5-star ratings
            Assert.Equal(1, summary.Distribution[9]);  // One 4.5-star rating
            Assert.Equal(1, summary.Distribution[8]);  // One 4-star rating
            Assert.Equal(1, summary.Distribution[5]);  // One 2.5-star rating
            Assert.Equal(5, summary.TotalRatings);
        }
    }
}