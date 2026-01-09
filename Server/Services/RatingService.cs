using Microsoft.EntityFrameworkCore;
using music_manager_starter.Data;
using music_manager_starter.Data.Models;
using music_manager_starter.Shared;

namespace music_manager_starter.Server.Services;

public sealed class RatingService : IRatingService
{
    private readonly DataDbContext _context;

    public RatingService(DataDbContext context)
    {
        _context = context;
    }

    public async Task RateSongAsync(Guid songId, double value, string userId)
    {
        if (value < 0 || value > 5 || value * 2 % 1 != 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Rating must be 0–5 in 0.5 steps");

        var rating = await _context.Ratings
            .SingleOrDefaultAsync(r => r.SongId == songId && r.UserId == userId);

        if (rating == null)
        {
            rating = new Rating
            {
                SongId = songId,
                UserId = userId,
                Value = value,
                CreatedAt = DateTime.UtcNow
            };
            _context.Ratings.Add(rating);
        }
        else
        {
            rating.Value = value;
            rating.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<RatingSummaryDto> GetSummaryAsync(Guid songId, string userId)
    {
        var ratings = _context.Ratings.Where(r => r.SongId == songId);

        var grouped = await ratings
            .GroupBy(r => r.Value)
            .Select(g => new { Value = g.Key, Count = g.Count() })
            .ToListAsync();

        var total = grouped.Sum(x => x.Count);
        var avg = total == 0 ? 0 : grouped.Sum(x => x.Value * x.Count) / total;

        var distribution = new int[11];
        foreach (var g in grouped)
        {
            distribution[(int)(g.Value * 2)] = g.Count;
        }

        var userRating = await ratings
            .Where(r => r.UserId == userId)
            .Select(r => (double?)r.Value)
            .FirstOrDefaultAsync();

        return new RatingSummaryDto
        {
            SongId = songId,
            Average = avg,
            TotalRatings = total,
            Distribution = distribution,
            UserRating = userRating
        };
    }
}
