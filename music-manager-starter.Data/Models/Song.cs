using System;

namespace music_manager_starter.Data.Models
{
    /// <summary>
    /// Represents a musical song in the system
    /// </summary>
    public sealed class Song
    {
        /// <summary>
        /// Unique identifier for the song
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Title of the song
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Artist or band who performs the song
        /// </summary>
        public string Artist { get; set; } = string.Empty;

        /// <summary>
        /// Album containing the song (if applicable)
        /// </summary>
        public string Album { get; set; } = string.Empty;

        /// <summary>
        /// Musical genre of the song
        /// </summary>
        public string Genre { get; set; } = string.Empty;

        /// <summary>
        /// Optional year the song was released.
        /// Nullable to support legacy data.
        /// </summary>
        public int? YearReleased { get; set; }
    }
}