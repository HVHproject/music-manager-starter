using System;

namespace music_manager_starter.Data.Models
{
    public sealed class Song
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Artist { get; set; } = string.Empty;

        public string Album { get; set; } = string.Empty;

        public string Genre { get; set; } = string.Empty;

        /// <summary>
        /// Optional year the song was released.
        /// Nullable to support legacy data.
        /// </summary>
        public int? YearReleased { get; set; }
    }
}