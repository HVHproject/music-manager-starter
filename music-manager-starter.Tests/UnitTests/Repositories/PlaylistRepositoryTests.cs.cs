using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using music_manager_starter.Data;
using music_manager_starter.Data.Models;
using music_manager_starter.Server.Repositories;
using Xunit;

namespace music_manager_starter.Tests.UnitTests.Repositories
{
    public class PlaylistRepositoryTests
    {
        private static DataDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<DataDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new DataDbContext(options);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingPlaylist_ReturnsPlaylist()
        {
            var context = CreateDbContext();
            var playlistId = Guid.NewGuid();
            var playlist = new Playlist
            {
                Id = playlistId,
                Name = "Test",
                CreatedByUserId = "user1",
                PlaylistSongs = new List<PlaylistSong>()
            };
            context.Playlists.Add(playlist);
            await context.SaveChangesAsync();

            var repo = new PlaylistRepository(context);
            var result = await repo.GetByIdAsync(playlistId, "user1");

            result.Should().NotBeNull();
            result!.Name.Should().Be("Test");
        }

        [Fact]
        public async Task GetByIdAsync_WrongUser_ReturnsNull()
        {
            var context = CreateDbContext();
            var playlist = new Playlist
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                CreatedByUserId = "user1",
                PlaylistSongs = new List<PlaylistSong>()
            };
            context.Playlists.Add(playlist);
            await context.SaveChangesAsync();

            var repo = new PlaylistRepository(context);
            var result = await repo.GetByIdAsync(playlist.Id, "user2");

            result.Should().BeNull();
        }

        [Fact]
        public async Task RemoveSongsAsync_RemovesAndReordersCorrectly()
        {
            var context = CreateDbContext();
            var playlistId = Guid.NewGuid();
            var song1Id = Guid.NewGuid();
            var song2Id = Guid.NewGuid();
            var song3Id = Guid.NewGuid();

            var playlist = new Playlist
            {
                Id = playlistId,
                Name = "Test",
                CreatedByUserId = "user1"
            };
            context.Playlists.Add(playlist);

            context.PlaylistSongs.AddRange(
                new PlaylistSong { PlaylistId = playlistId, SongId = song1Id, OrderIndex = 0 },
                new PlaylistSong { PlaylistId = playlistId, SongId = song2Id, OrderIndex = 1 },
                new PlaylistSong { PlaylistId = playlistId, SongId = song3Id, OrderIndex = 2 }
            );
            await context.SaveChangesAsync();

            var repo = new PlaylistRepository(context);
            await repo.RemoveSongsAsync(playlistId, new List<Guid> { song2Id });
            await repo.SaveChangesAsync();

            var remaining = await context.PlaylistSongs
                .Where(ps => ps.PlaylistId == playlistId)
                .OrderBy(ps => ps.OrderIndex)
                .ToListAsync();

            remaining.Should().HaveCount(2);
            remaining[0].SongId.Should().Be(song1Id);
            remaining[0].OrderIndex.Should().Be(0);
            remaining[1].SongId.Should().Be(song3Id);
            remaining[1].OrderIndex.Should().Be(1);
        }

        [Fact]
        public async Task ReplaceSongsAsync_UpdatesOrderCorrectly()
        {
            var context = CreateDbContext();
            var playlistId = Guid.NewGuid();
            var song1Id = Guid.NewGuid();
            var song2Id = Guid.NewGuid();

            var playlist = new Playlist
            {
                Id = playlistId,
                Name = "Test",
                CreatedByUserId = "user1"
            };
            context.Playlists.Add(playlist);

            context.PlaylistSongs.AddRange(
                new PlaylistSong { PlaylistId = playlistId, SongId = song1Id, OrderIndex = 0 },
                new PlaylistSong { PlaylistId = playlistId, SongId = song2Id, OrderIndex = 1 }
            );
            await context.SaveChangesAsync();

            var repo = new PlaylistRepository(context);
            var newOrder = new List<PlaylistSong>
            {
                new PlaylistSong { PlaylistId = playlistId, SongId = song2Id },
                new PlaylistSong { PlaylistId = playlistId, SongId = song1Id }
            };
            await repo.ReplaceSongsAsync(playlistId, newOrder);

            var reordered = await context.PlaylistSongs
                .Where(ps => ps.PlaylistId == playlistId)
                .OrderBy(ps => ps.OrderIndex)
                .ToListAsync();

            reordered[0].SongId.Should().Be(song2Id);
            reordered[0].OrderIndex.Should().Be(0);
            reordered[1].SongId.Should().Be(song1Id);
            reordered[1].OrderIndex.Should().Be(1);
        }
    }
}