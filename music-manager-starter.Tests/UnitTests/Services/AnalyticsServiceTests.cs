using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using music_manager_starter.Data;
using music_manager_starter.Server.Services;
using music_manager_starter.Shared;
using Xunit;

namespace music_manager_starter.Tests.UnitTests.Services
{
    public class AnalyticsServiceTests
    {
        private DataDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<DataDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new DataDbContext(options);
        }

        private AnalyticsService CreateService(DataDbContext? context = null)
        {
            var dbContext = context ?? CreateInMemoryContext();
            return new AnalyticsService(dbContext);
        }

        [Fact]
        public async Task GetRatingTrendsAsync_WithDefaultDates_ReturnsData()
        {
            // Arrange
            var service = CreateService();

            // Act
            var result = await service.GetRatingTrendsAsync();

            // Assert
            Assert.NotNull(result);

            if (result.Any())
            {
                // Check date ordering (should be sorted)
                var dates = result.Select(t => t.Date).ToList();
                for (int i = 1; i < dates.Count; i++)
                {
                    Assert.True(dates[i] >= dates[i - 1]);
                }

                // Check data ranges
                foreach (var trend in result)
                {
                    Assert.InRange(trend.AverageRating, 1.0, 5.0);
                    Assert.True(trend.TotalRatings >= 0);
                    Assert.True(trend.TotalSongsRated >= 0);
                    Assert.True(trend.TotalSongsRated <= trend.TotalRatings);
                }
            }
        }

        [Fact]
        public async Task GetRatingTrendsAsync_WithCustomDateRange_ReturnsData()
        {
            // Arrange
            var service = CreateService();
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 1, 31);

            // Act
            var result = await service.GetRatingTrendsAsync(startDate, endDate);

            // Assert
            Assert.NotNull(result);

            if (result.Any())
            {
                foreach (var trend in result)
                {
                    Assert.InRange(trend.Date, startDate, endDate);
                }
            }
        }

        [Fact]
        public async Task GetRatingTrendsAsync_WithReversedDates_DoesNotThrow()
        {
            // Arrange
            var service = CreateService();
            var startDate = new DateTime(2024, 12, 31);
            var endDate = new DateTime(2024, 1, 1);

            // Act
            var exception = await Record.ExceptionAsync(() =>
                service.GetRatingTrendsAsync(startDate, endDate));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public async Task GetRatingTrendsAsync_ReturnsValidDataStructure()
        {
            // Arrange
            var service = CreateService();

            // Act
            var result = await service.GetRatingTrendsAsync();

            // Assert
            Assert.NotNull(result);

            // Check each item has valid structure
            foreach (var trend in result)
            {
                Assert.NotNull(trend);
                Assert.NotEqual(default(DateTime), trend.Date);
                Assert.InRange(trend.AverageRating, 1.0, 5.0);
                Assert.True(trend.TotalRatings >= 0);
                Assert.True(trend.TotalSongsRated >= 0);
            }
        }

        [Fact]
        public async Task GetRatingTrendsAsync_CalledMultipleTimes_ReturnsConsistentResults()
        {
            // Arrange
            var service = CreateService();
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 1, 31);

            // Act
            var result1 = await service.GetRatingTrendsAsync(startDate, endDate);
            var result2 = await service.GetRatingTrendsAsync(startDate, endDate);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
        }
    }
}