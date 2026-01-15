using Microsoft.EntityFrameworkCore;
using music_manager_starter.Data;
using music_manager_starter.Server.Services;
using music_manager_starter.Shared;
using Xunit;
using FluentAssertions;

namespace music_manager_starter.Tests.Services
{
    public class SongServiceTests
    {
        private static DataDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<DataDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new DataDbContext(options);
        }

        [Fact]
        public async Task SearchSongsAsync_NoSongs_ReturnsEmpty()
        {
            var context = CreateDbContext();
            var service = new SongService(context);

            var request = new SongSearchRequest
            {
                Query = "test",
                PageSize = 10
            };

            var result = await service.SearchSongsAsync(request, null);

            result.Results.Should().BeEmpty();
            result.NextCursor.Should().BeNull();
        }

        [Fact]
        public async Task SearchSongsAsync_TextQuery_FiltersCorrectly()
        {
            var context = CreateDbContext();

            var song1 = new Data.Models.Song
            {
                Id = Guid.NewGuid(),
                Title = "Hello World",
                Artist = "Artist A",
                Album = "Album A",
                Genre = "Rock",
                YearReleased = 2020
            };

            var song2 = new Data.Models.Song
            {
                Id = Guid.NewGuid(),
                Title = "Goodbye",
                Artist = "Artist B",
                Album = "Album B",
                Genre = "Pop",
                YearReleased = 2021
            };

            context.Songs.AddRange(song1, song2);
            await context.SaveChangesAsync();

            var service = new SongService(context);

            var request = new SongSearchRequest
            {
                Query = "hello",
                PageSize = 10
            };

            var result = await service.SearchSongsAsync(request, null);

            result.Results.Should().HaveCount(1);
            result.Results[0].Title.Should().Be("Hello World");
        }

        [Fact]
        public async Task SearchSongsAsync_MinRating_FiltersCorrectly()
        {
            var context = CreateDbContext();

            var song1 = new Data.Models.Song
            {
                Id = Guid.NewGuid(),
                Title = "High Rated",
                Artist = "Artist A",
                Album = "Album A",
                Genre = "Rock",
                YearReleased = 2020
            };

            var song2 = new Data.Models.Song
            {
                Id = Guid.NewGuid(),
                Title = "Low Rated",
                Artist = "Artist B",
                Album = "Album B",
                Genre = "Rock",
                YearReleased = 2020
            };

            context.Songs.AddRange(song1, song2);

            context.Ratings.AddRange(
                new Data.Models.Rating { SongId = song1.Id, UserId = "u1", Value = 5 },
                new Data.Models.Rating { SongId = song1.Id, UserId = "u2", Value = 4 },
                new Data.Models.Rating { SongId = song2.Id, UserId = "u1", Value = 2 }
            );

            await context.SaveChangesAsync();

            var service = new SongService(context);

            var request = new SongSearchRequest
            {
                MinRating = 4.0,
                PageSize = 10
            };

            var result = await service.SearchSongsAsync(request, null);

            result.Results.Should().HaveCount(1);
            result.Results[0].Title.Should().Be("High Rated");
            result.Results[0].AverageRating.Should().BeGreaterThanOrEqualTo(4.0);
        }
    }
}
