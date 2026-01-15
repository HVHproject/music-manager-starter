using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using music_manager_starter.Server.Controllers;
using music_manager_starter.Server.Services;
using music_manager_starter.Shared;
using System.Security.Claims;
using Xunit;

namespace music_manager_starter.Tests.Controllers
{
    public class SongsControllerTests
    {
        private static SongsController CreateController(
            Mock<ISongService> serviceMock,
            string? userId = "test-user")
        {
            var controller = new SongsController(serviceMock.Object);

            var claims = new List<Claim>();
            if (userId != null)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
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
        public async Task GetSongs_ReturnsOkWithSongs()
        {
            var mockService = new Mock<ISongService>();

            var songs = new List<Song>
            {
                new Song { Title = "Song A" },
                new Song { Title = "Song B" }
            };

            mockService
                .Setup(s => s.GetSongsAsync(It.IsAny<string>()))
                .ReturnsAsync(songs);

            var controller = CreateController(mockService);

            var result = await controller.GetSongs();

            var okResult = result.Result as OkObjectResult;

            okResult.Should().NotBeNull();
            okResult!.Value.Should().BeEquivalentTo(songs);
        }

        [Fact]
        public async Task PostSong_ValidSong_CallsServiceAndReturnsOk()
        {
            var mockService = new Mock<ISongService>();

            var controller = CreateController(mockService);

            var song = new Song
            {
                Title = "New Song",
                Artist = "Artist",
                Album = "Album",
                Genre = "Rock",
                YearReleased = 2024
            };

            var result = await controller.PostSong(song);

            result.Should().BeOfType<OkResult>();

            mockService.Verify(
                s => s.AddSongAsync(It.Is<Song>(x => x.Title == "New Song")),
                Times.Once);
        }

        [Fact]
        public async Task SearchSongs_ReturnsOkWithResponse()
        {
            var mockService = new Mock<ISongService>();

            var response = new SongSearchResponse
            {
                Results = new List<Song>
                {
                    new Song { Title = "Match" }
                }
            };

            mockService
                .Setup(s => s.SearchSongsAsync(It.IsAny<SongSearchRequest>(), It.IsAny<string>()))
                .ReturnsAsync(response);

            var controller = CreateController(mockService);

            var request = new SongSearchRequest
            {
                Query = "match",
                PageSize = 10
            };

            var result = await controller.SearchSongs(request);

            var okResult = result.Result as OkObjectResult;

            okResult.Should().NotBeNull();
            okResult!.Value.Should().BeEquivalentTo(response);
        }
    }
}
