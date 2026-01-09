using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using music_manager_starter.Data;
using music_manager_starter.Data.Models;
using System;
using System.Security.Claims;
using music_manager_starter.Shared;

namespace music_manager_starter.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SongsController : ControllerBase
    {
        private readonly DataDbContext _context;

        public SongsController(DataDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<music_manager_starter.Shared.Song>>> GetSongs()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var songsList = await _context.Songs.ToListAsync();
            var ratingsList = await _context.Ratings.ToListAsync();

            var songs = songsList.Select(s => {
                var songRatings = ratingsList.Where(r => r.SongId == s.Id);
                var totalRatings = songRatings.Count();
                var average = totalRatings > 0 ? songRatings.Average(r => (double)r.Value) : (double?)null;
                var userRating = songRatings.FirstOrDefault(r => r.UserId == userId)?.Value;
                return new music_manager_starter.Shared.Song
                {
                    Id = s.Id,
                    Title = s.Title,
                    Artist = s.Artist,
                    Album = s.Album,
                    Genre = s.Genre,
                    AverageRating = average,
                    UserRating = userRating.HasValue ? (double?)userRating.Value : null,
                    TotalRatings = totalRatings
                };
            }).ToList();

            return Ok(songs);
        }
    }
}
