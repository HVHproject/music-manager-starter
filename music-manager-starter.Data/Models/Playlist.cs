using System;
using System.Collections.Generic;

namespace music_manager_starter.Data.Models
{
    /// <summary>
    /// Represents a user-created collection of songs
    /// </summary>
    public sealed class Playlist
    {
        /// <summary>
        /// Unique identifier for the playlist
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Display name of the playlist
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// ID of the user who created the playlist
        /// Based on authentication from Fake User Middleware
        /// </summary>
        public string CreatedByUserId { get; set; } = string.Empty;

        public string? UpdatedByUserId { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when the playlist was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Timestamp when the playlist was last modified
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Collection of songs in this playlist with their order
        /// </summary>
        public ICollection<PlaylistSong> PlaylistSongs { get; set; }
            = new List<PlaylistSong>();
    }
}