using System;
using System.ComponentModel.DataAnnotations;

namespace music_manager_starter.Data.Models
{
    /// <summary>
    /// Represents a single user's rating for a song.
    /// One rating per user per song is enforced at the database level.
    /// </summary>
    public sealed class Rating
    {
        /// <summary>
        /// Primary key for the rating.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Foreign key to the rated song.
        /// </summary>
        public Guid SongId { get; set; }

        /// <summary>
        /// Identifier of the user who submitted the rating.
        /// Sourced from authenticated user claims.
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Rating value (0–5.0 in 0.5 increments).
        /// </summary>
        [Range(0, 5.0)]
        public decimal Value { get; set; }

        /// <summary>
        /// Timestamp when the rating was created or last updated.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Navigation property to the related song.
        /// </summary>
        public Song Song { get; set; } = null!;
    }
}
