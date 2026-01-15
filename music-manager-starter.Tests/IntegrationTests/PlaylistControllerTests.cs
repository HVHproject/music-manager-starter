using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using music_manager_starter.Server.Controllers;
using music_manager_starter.Server.Services;
using music_manager_starter.Shared;
using System.Security.Claims;
using Xunit;

namespace music_manager_starter.Tests.IntegrationTests
{
    public class PlaylistControllerTests
    {
        private static PlaylistController CreateController(
            Mock<IPlaylistService> serviceMock,
            string? userId = "test-user")
        {
            var controller = new PlaylistController(serviceMock.Object);
            var claims = new List<Claim>();
            if (userId != null)
            {
                claims.Add(new Claim(ClaimTypes.Name, userId));
            }
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims))
                }
            };
            return controller;
        }

        [Fact]
        public async Task Create_ValidName_ReturnsOkWithGuid()
        {
            var mockService = new Mock<IPlaylistService>();
            var expectedId = Guid.NewGuid();
            mockService
                .Setup(s => s.CreatePlaylistAsync("My Playlist", "test-user"))
                .ReturnsAsync(expectedId);

            var controller = CreateController(mockService);
            var result = await controller.Create("My Playlist");

            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.Value.Should().Be(expectedId);
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithPlaylists()
        {
            var mockService = new Mock<IPlaylistService>();
            var playlists = new List<PlaylistDto>
            {
                new PlaylistDto { Id = Guid.NewGuid(), Name = "Playlist A" },
                new PlaylistDto { Id = Guid.NewGuid(), Name = "Playlist B" }
            };
            mockService
                .Setup(s => s.GetAllPlaylistsAsync("test-user"))
                .ReturnsAsync(playlists);

            var controller = CreateController(mockService);
            var result = await controller.GetAll();

            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.Value.Should().BeEquivalentTo(playlists);
        }

        [Fact]
        public async Task Get_ExistingPlaylist_ReturnsOkWithPlaylist()
        {
            var mockService = new Mock<IPlaylistService>();
            var playlistId = Guid.NewGuid();
            var playlist = new PlaylistDto { Id = playlistId, Name = "Test Playlist" };
            mockService
                .Setup(s => s.GetPlaylistAsync(playlistId, "test-user"))
                .ReturnsAsync(playlist);

            var controller = CreateController(mockService);
            var result = await controller.Get(playlistId);

            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.Value.Should().BeEquivalentTo(playlist);
        }

        [Fact]
        public async Task Rename_ValidRequest_ReturnsNoContent()
        {
            var mockService = new Mock<IPlaylistService>();
            var playlistId = Guid.NewGuid();

            var controller = CreateController(mockService);
            var result = await controller.Rename(playlistId, "New Name");

            result.Should().BeOfType<NoContentResult>();
            mockService.Verify(
                s => s.RenamePlaylistAsync(playlistId, "New Name", "test-user"),
                Times.Once);
        }

        [Fact]
        public async Task Delete_ExistingPlaylist_ReturnsNoContent()
        {
            var mockService = new Mock<IPlaylistService>();
            var playlistId = Guid.NewGuid();

            var controller = CreateController(mockService);
            var result = await controller.Delete(playlistId);

            result.Should().BeOfType<NoContentResult>();
            mockService.Verify(
                s => s.DeletePlaylistAsync(playlistId, "test-user"),
                Times.Once);
        }

        [Fact]
        public async Task AddSongs_ValidRequest_ReturnsNoContent()
        {
            var mockService = new Mock<IPlaylistService>();
            var playlistId = Guid.NewGuid();
            var songIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            var controller = CreateController(mockService);
            var result = await controller.AddSongs(playlistId, songIds);

            result.Should().BeOfType<NoContentResult>();
            mockService.Verify(
                s => s.AddSongsAsync(playlistId, songIds, "test-user"),
                Times.Once);
        }

        [Fact]
        public async Task RemoveSongs_ValidRequest_ReturnsNoContent()
        {
            var mockService = new Mock<IPlaylistService>();
            var playlistId = Guid.NewGuid();
            var songIds = new List<Guid> { Guid.NewGuid() };

            var controller = CreateController(mockService);
            var result = await controller.RemoveSongs(playlistId, songIds);

            result.Should().BeOfType<NoContentResult>();
            mockService.Verify(
                s => s.RemoveSongsAsync(playlistId, songIds, "test-user"),
                Times.Once);
        }

        [Fact]
        public async Task Reorder_ValidRequest_ReturnsNoContent()
        {
            var mockService = new Mock<IPlaylistService>();
            var playlistId = Guid.NewGuid();
            var orderedIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            var controller = CreateController(mockService);
            var result = await controller.Reorder(playlistId, orderedIds);

            result.Should().BeOfType<NoContentResult>();
            mockService.Verify(
                s => s.ReorderSongsAsync(playlistId, orderedIds, "test-user"),
                Times.Once);
        }

        [Fact]
        public async Task Export_CsvFormat_ReturnsOkWithCsv()
        {
            var mockService = new Mock<IPlaylistService>();
            var playlistId = Guid.NewGuid();
            var csvData = "Title,Artist,Album\nSong1,Artist1,Album1";
            mockService
                .Setup(s => s.ExportAsync(playlistId, "csv", "test-user"))
                .ReturnsAsync(csvData);

            var controller = CreateController(mockService);
            var result = await controller.Export(playlistId, "csv");

            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.Value.Should().Be(csvData);
        }
    }
}