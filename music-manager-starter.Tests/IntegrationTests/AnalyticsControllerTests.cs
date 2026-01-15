using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using music_manager_starter.Server.Controllers;
using music_manager_starter.Server.Services;
using FluentAssertions;
using Xunit;

namespace music_manager_starter.Tests.IntegrationTests
{
    public class AnalyticsControllerTests
    {
        private readonly Mock<IAnalyticsService> _mockAnalyticsService;
        private readonly AnalyticsController _controller;

        public AnalyticsControllerTests()
        {
            _mockAnalyticsService = new Mock<IAnalyticsService>();
            _controller = new AnalyticsController(_mockAnalyticsService.Object);
        }

        [Fact]
        public async Task GetMostRatedSongs_ValidLimit_ReturnsOk()
        {
            // Arrange
            var topN = 5;

            var song1 = new MostRatedSongDto();
            SetPropertyIfExists(song1, "SongId", Guid.NewGuid());
            SetPropertyIfExists(song1, "Title", "Song 1");
            SetPropertyIfExists(song1, "AverageRating", 4.5);
            SetPropertyIfExists(song1, "TotalRatings", 10);
            SetPropertyIfExists(song1, "RatingCount", 10);

            var song2 = new MostRatedSongDto();
            SetPropertyIfExists(song2, "SongId", Guid.NewGuid());
            SetPropertyIfExists(song2, "Title", "Song 2");
            SetPropertyIfExists(song2, "AverageRating", 4.2);
            SetPropertyIfExists(song2, "TotalRatings", 8);
            SetPropertyIfExists(song2, "RatingCount", 8);

            var expectedSongs = new List<MostRatedSongDto> { song1, song2 };

            _mockAnalyticsService.Setup(s => s.GetMostRatedSongsAsync(topN))
                .ReturnsAsync(expectedSongs);

            // Act
            var result = await _controller.GetMostRatedSongs(topN);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetRatingTrends_WithDates_ReturnsOk()
        {
            // Arrange
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 1, 31);

            var trend1 = new RatingTrendDto();
            SetPropertyIfExists(trend1, "Date", new DateTime(2024, 1, 10));
            SetPropertyIfExists(trend1, "AverageRating", 4.2);
            SetPropertyIfExists(trend1, "TotalRatings", 15);
            SetPropertyIfExists(trend1, "TotalSongsRated", 5);
            SetPropertyIfExists(trend1, "RatingCount", 15);

            var trend2 = new RatingTrendDto();
            SetPropertyIfExists(trend2, "Date", new DateTime(2024, 1, 20));
            SetPropertyIfExists(trend2, "AverageRating", 4.5);
            SetPropertyIfExists(trend2, "TotalRatings", 22);
            SetPropertyIfExists(trend2, "TotalSongsRated", 8);
            SetPropertyIfExists(trend2, "RatingCount", 22);

            var expectedTrends = new List<RatingTrendDto> { trend1, trend2 };

            _mockAnalyticsService.Setup(s => s.GetRatingTrendsAsync(startDate, endDate))
                .ReturnsAsync(expectedTrends);

            // Act
            var result = await _controller.GetRatingTrends(startDate, endDate);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetGenrePopularity_ReturnsOk()
        {
            // Arrange
            var genre1 = new GenrePopularityDto();
            SetPropertyIfExists(genre1, "Genre", "Rock");
            SetPropertyIfExists(genre1, "Count", 15);
            SetPropertyIfExists(genre1, "SongCount", 15);
            SetPropertyIfExists(genre1, "TotalSongs", 15);

            var genre2 = new GenrePopularityDto();
            SetPropertyIfExists(genre2, "Genre", "Pop");
            SetPropertyIfExists(genre2, "Count", 12);
            SetPropertyIfExists(genre2, "SongCount", 12);

            var genre3 = new GenrePopularityDto();
            SetPropertyIfExists(genre3, "Genre", "Jazz");
            SetPropertyIfExists(genre3, "Count", 8);
            SetPropertyIfExists(genre3, "SongCount", 8);

            var expectedGenres = new List<GenrePopularityDto> { genre1, genre2, genre3 };

            _mockAnalyticsService.Setup(s => s.GetGenrePopularityAsync())
                .ReturnsAsync(expectedGenres);

            // Act
            var result = await _controller.GetGenrePopularity();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetSummary_ReturnsOk()
        {
            // Arrange
            var summary = new AnalyticsSummaryDto();
            SetPropertyIfExists(summary, "TotalSongs", 50);
            SetPropertyIfExists(summary, "TotalRatings", 250);
            SetPropertyIfExists(summary, "AverageRating", 4.2);
            SetPropertyIfExists(summary, "MostPopularGenre", "Rock");
            SetPropertyIfExists(summary, "TopGenre", "Rock");

            _mockAnalyticsService.Setup(s => s.GetAnalyticsSummaryAsync())
                .ReturnsAsync(summary);

            // Act
            var result = await _controller.GetSummary();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        // Helper method to set properties if they exist
        private void SetPropertyIfExists(object obj, string propertyName, object value)
        {
            var property = obj.GetType().GetProperty(propertyName);
            if (property != null && property.CanWrite)
            {
                property.SetValue(obj, value);
            }
        }
    }
}