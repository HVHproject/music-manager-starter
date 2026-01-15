using FluentAssertions;
using Moq;
using music_manager_starter.Data.Models;
using music_manager_starter.Server.Repositories;
using music_manager_starter.Server.Services;
using music_manager_starter.Shared;
using Xunit;

namespace music_manager_starter.Tests.UnitTests.Services
{
    public class PlaylistServiceTests
    {
        [Fact]
        public async Task CreatePlaylistAsync_ValidName_ReturnsGuid()
        {
            var mockRepo = new Mock<IPlaylistRepository>();
            var service = new PlaylistService(mockRepo.Object);

            var result = await service.CreatePlaylistAsync("My Playlist", "user1");

            result.Should().NotBeEmpty();
            mockRepo.Verify(r => r.AddAsync(It.Is<Playlist>(p => p.Name == "My Playlist")), Times.Once);
            mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllPlaylistsAsync_ReturnsPlaylists()
        {
            var mockRepo = new Mock<IPlaylistRepository>();
            var playlists = new List<Playlist>
            {
                new Playlist
                {
                    Id = Guid.NewGuid(),
                    Name = "Playlist 1",
                    CreatedByUserId = "user1",
                    PlaylistSongs = new List<PlaylistSong>()
                }
            };
            mockRepo.Setup(r => r.GetAllByUserIdAsync("user1")).ReturnsAsync(playlists);

            var service = new PlaylistService(mockRepo.Object);
            var result = await service.GetAllPlaylistsAsync("user1");

            result.Should().HaveCount(1);
            result[0].Name.Should().Be("Playlist 1");
        }

        [Fact]
        public async Task GetPlaylistAsync_ExistingPlaylist_ReturnsDto()
        {
            var mockRepo = new Mock<IPlaylistRepository>();
            var playlistId = Guid.NewGuid();
            var playlist = new Playlist
            {
                Id = playlistId,
                Name = "Test Playlist",
                CreatedByUserId = "user1",
                PlaylistSongs = new List<PlaylistSong>()
            };
            mockRepo.Setup(r => r.GetByIdAsync(playlistId, "user1")).ReturnsAsync(playlist);

            var service = new PlaylistService(mockRepo.Object);
            var result = await service.GetPlaylistAsync(playlistId, "user1");

            result.Should().NotBeNull();
            result.Name.Should().Be("Test Playlist");
        }

        [Fact]
        public async Task GetPlaylistAsync_NonExistingPlaylist_ThrowsKeyNotFoundException()
        {
            var mockRepo = new Mock<IPlaylistRepository>();
            mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync((Playlist?)null);

            var service = new PlaylistService(mockRepo.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.GetPlaylistAsync(Guid.NewGuid(), "user1"));
        }

        [Fact]
        public async Task RenamePlaylistAsync_ValidRequest_UpdatesName()
        {
            var mockRepo = new Mock<IPlaylistRepository>();
            var playlistId = Guid.NewGuid();
            var playlist = new Playlist
            {
                Id = playlistId,
                Name = "Old Name",
                CreatedByUserId = "user1",
                PlaylistSongs = new List<PlaylistSong>()
            };
            mockRepo.Setup(r => r.GetByIdAsync(playlistId, "user1")).ReturnsAsync(playlist);

            var service = new PlaylistService(mockRepo.Object);
            await service.RenamePlaylistAsync(playlistId, "New Name", "user1");

            playlist.Name.Should().Be("New Name");
            mockRepo.Verify(r => r.UpdateAsync(playlist), Times.Once);
            mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AddSongsAsync_NewSongs_AddsToPlaylist()
        {
            var mockRepo = new Mock<IPlaylistRepository>();
            var playlistId = Guid.NewGuid();
            var playlist = new Playlist
            {
                Id = playlistId,
                Name = "Playlist",
                CreatedByUserId = "user1",
                PlaylistSongs = new List<PlaylistSong>()
            };
            mockRepo.Setup(r => r.GetByIdAsync(playlistId, "user1")).ReturnsAsync(playlist);

            var service = new PlaylistService(mockRepo.Object);
            var songIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            await service.AddSongsAsync(playlistId, songIds, "user1");

            playlist.PlaylistSongs.Should().HaveCount(2);
            mockRepo.Verify(r => r.UpdateAsync(playlist), Times.Once);
            mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ExportAsync_CsvFormat_ReturnsCsvString()
        {
            var mockRepo = new Mock<IPlaylistRepository>();
            var playlistId = Guid.NewGuid();
            var song = new Data.Models.Song
            {
                Id = Guid.NewGuid(),
                Title = "Test Song",
                Artist = "Test Artist",
                Album = "Test Album",
                Genre = "Rock",
                YearReleased = 2020
            };
            var playlist = new Playlist
            {
                Id = playlistId,
                Name = "Playlist",
                CreatedByUserId = "user1",
                PlaylistSongs = new List<PlaylistSong>
                {
                    new PlaylistSong
                    {
                        PlaylistId = playlistId,
                        SongId = song.Id,
                        OrderIndex = 0,
                        Song = song
                    }
                }
            };
            mockRepo.Setup(r => r.GetByIdAsync(playlistId, "user1")).ReturnsAsync(playlist);

            var service = new PlaylistService(mockRepo.Object);
            var result = await service.ExportAsync(playlistId, "csv", "user1");

            result.Should().Contain("Title,Artist,Album,Genre,YearReleased");
            result.Should().Contain("Test Song,Test Artist,Test Album,Rock,2020");
        }
    }
}