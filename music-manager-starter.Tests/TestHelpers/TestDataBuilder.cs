using System;
using System.Collections.Generic;
using System.Linq;
using music_manager_starter.Data.Models;
using music_manager_starter.Shared;

namespace music_manager_starter.Tests.TestHelpers
{
    public class TestDataBuilder
    {
        private readonly Random _random = new Random();

        public Data.Models.Song CreateSong(Guid? id = null, string title = "")
        {
            return new Data.Models.Song
            {
                Id = id ?? Guid.NewGuid(),
                Title = title ?? $"Test Song {_random.Next(1000)}",
                Artist = $"Test Artist {_random.Next(100)}",
                Album = $"Test Album {_random.Next(50)}",
                Genre = _random.Next(3) switch
                {
                    0 => "Rock",
                    1 => "Pop",
                    2 => "Jazz",
                    _ => "Other"
                }
            };
        }

        public Rating CreateRating(Guid songId, string userId, decimal? value = null)
        {
            return new Rating
            {
                SongId = songId,
                UserId = userId,
                Value = value ?? (decimal)(_random.Next(1, 10) * 0.5m), // 0.5 to 5.0 in 0.5 increments
                CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(30))
            };
        }

        public List<Rating> CreateRatings(Guid songId, int count, string baseUserId = "user")
        {
            var ratings = new List<Rating>();
            for (int i = 0; i < count; i++)
            {
                ratings.Add(CreateRating(songId, $"{baseUserId}-{i}"));
            }
            return ratings;
        }

        public music_manager_starter.Shared.Song CreateSharedSong(Guid? id = null)
        {
            var avgRating = _random.NextDouble() * 4 + 1; // 1.0 to 5.0
            var totalRatings = _random.Next(1, 100);

            return new music_manager_starter.Shared.Song
            {
                Id = id ?? Guid.NewGuid(),
                Title = $"Shared Test Song {_random.Next(1000)}",
                Artist = $"Shared Test Artist {_random.Next(100)}",
                Album = $"Shared Test Album {_random.Next(50)}",
                Genre = "Test Genre",
                AverageRating = Math.Round(avgRating, 1),
                TotalRatings = totalRatings,
                UserRating = _random.Next(3) == 0 ? (double?)_random.Next(1, 10) * 0.5 : null
            };
        }

        public List<music_manager_starter.Shared.Song> CreateSharedSongs(int count)
        {
            var songs = new List<music_manager_starter.Shared.Song>();
            for (int i = 0; i < count; i++)
            {
                songs.Add(CreateSharedSong());
            }
            return songs;
        }

        public RatingTrendDto CreateRatingTrend(DateTime date)
        {
            return new RatingTrendDto
            {
                Date = date,
                AverageRating = Math.Round(_random.NextDouble() * 4 + 1, 2), // 1.0 to 5.0
                TotalRatings = _random.Next(1, 50),
                TotalSongsRated = _random.Next(1, 20)
            };
        }

        public List<RatingTrendDto> CreateRatingTrends(DateTime startDate, DateTime endDate, int points = 10)
        {
            var trends = new List<RatingTrendDto>();
            var totalDays = (endDate - startDate).Days;

            for (int i = 0; i < points; i++)
            {
                var daysOffset = _random.Next(0, totalDays);
                var date = startDate.AddDays(daysOffset);

                // Ensure unique dates
                while (trends.Any(t => t.Date.Date == date.Date))
                {
                    date = date.AddDays(1);
                }

                trends.Add(CreateRatingTrend(date));
            }

            // Sort by date
            trends.Sort((a, b) => a.Date.CompareTo(b.Date));

            return trends;
        }
    }
}