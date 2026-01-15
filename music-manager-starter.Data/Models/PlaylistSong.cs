using System;

namespace music_manager_starter.Data.Models
{
    /// <summary>
    /// Join entity representing a song within a playlist
    /// Maintains the order of songs in a playlist
    /// </summary>
    public sealed class PlaylistSong
    {
        /// <summary>
        /// Foreign key to the playlist
        /// </summary>
        public Guid PlaylistId { get; set; }

        /// <summary>
        /// Navigation property to the playlist
        /// </summary>
        public Playlist Playlist { get; set; } = null!;

        /// <summary>
        /// Foreign key to the song
        /// </summary>
        public Guid SongId { get; set; }

        /// <summary>
        /// Navigation property to the song
        /// </summary>
        public Song Song { get; set; } = null!;

        /// <summary>
        /// Position of the song within the playlist (0-based index)
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// Timestamp when the song was added to the playlist
        /// </summary>
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}