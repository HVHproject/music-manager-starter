using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using music_manager_starter.Server.Services;

namespace music_manager_starter.Server.Controllers
{
    /// <summary>
    /// Controller for analytics data about songs and ratings
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsController"/> class
        /// </summary>
        /// <param name="analyticsService">The analytics service</param>
        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        /// <summary>
        /// Gets the most rated songs
        /// </summary>
        /// <param name="topN">Number of songs to return (default: 10)</param>
        /// <returns>List of most rated songs</returns>
        /// <response code="200">Returns the most rated songs</response>
        [HttpGet("most-rated/{topN?}")]
        public async Task<IActionResult> GetMostRatedSongs(int topN = 10)
        {
            var songs = await _analyticsService.GetMostRatedSongsAsync(topN);
            return Ok(songs);
        }

        /// <summary>
        /// Gets rating trends over time
        /// </summary>
        /// <param name="startDate">Start date for the trend analysis</param>
        /// <param name="endDate">End date for the trend analysis</param>
        /// <returns>List of rating trends by date</returns>
        /// <response code="200">Returns the rating trends</response>
        [HttpGet("trends")]
        public async Task<IActionResult> GetRatingTrends([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var trends = await _analyticsService.GetRatingTrendsAsync(startDate, endDate);
            return Ok(trends);
        }

        /// <summary>
        /// Gets genre popularity based on ratings
        /// </summary>
        /// <returns>List of genres with their popularity scores</returns>
        /// <response code="200">Returns the genre popularity data</response>
        [HttpGet("genres")]
        public async Task<IActionResult> GetGenrePopularity()
        {
            var genres = await _analyticsService.GetGenrePopularityAsync();
            return Ok(genres);
        }

        /// <summary>
        /// Gets an overall analytics summary
        /// </summary>
        /// <returns>Summary of all analytics data</returns>
        /// <response code="200">Returns the analytics summary</response>
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var summary = await _analyticsService.GetAnalyticsSummaryAsync();
            return Ok(summary);
        }
    }
}