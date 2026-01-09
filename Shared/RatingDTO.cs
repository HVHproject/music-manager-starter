namespace music_manager_starter.Shared;

public sealed class RatingSummaryDto
{
    public Guid SongId { get; init; }
    public double Average { get; init; }
    public int TotalRatings { get; init; }

    // 0 = 0 stars, 1 = 0.5 stars, ..., 10 = 5 stars
    public int[] Distribution { get; init; } = new int[11];

    public double? UserRating { get; init; }
}
