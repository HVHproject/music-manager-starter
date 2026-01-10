using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using music_manager_starter.Server.Services;

namespace music_manager_starter.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet("most-rated/{topN?}")]
        public async Task<IActionResult> GetMostRatedSongs(int topN = 10)
        {
            var songs = await _analyticsService.GetMostRatedSongsAsync(topN);
            return Ok(songs);
        }

        [HttpGet("trends")]
        public async Task<IActionResult> GetRatingTrends([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var trends = await _analyticsService.GetRatingTrendsAsync(startDate, endDate);
            return Ok(trends);
        }

        [HttpGet("genres")]
        public async Task<IActionResult> GetGenrePopularity()
        {
            var genres = await _analyticsService.GetGenrePopularityAsync();
            return Ok(genres);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var summary = await _analyticsService.GetAnalyticsSummaryAsync();
            return Ok(summary);
        }
    }
}