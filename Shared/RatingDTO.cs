namespace music_manager_starter.Shared;

/// <summary>
/// Data transfer object for rating summary
/// </summary>
public sealed class RatingSummaryDto
{
    /// <summary>Gets or sets the song identifier</summary>
    public Guid SongId { get; init; }

    /// <summary>Gets or sets the average rating</summary>
    public double Average { get; init; }

    /// <summary>Gets or sets the total number of ratings</summary>
    public int TotalRatings { get; init; }

    /// <summary>
    /// Gets or sets the rating distribution
    /// Index 0 = 0 stars, 1 = 0.5 stars, ..., 10 = 5 stars
    /// </summary>
    public int[] Distribution { get; init; } = new int[11];

    /// <summary>Gets or sets the current user's rating (if any)</summary>
    public double? UserRating { get; init; }

    /// <summary>Gets or sets when the rating was created</summary>
    public DateTime? CreatedAt { get; set; }
}