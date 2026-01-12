using System;

namespace music_manager_starter.Data.Models
{
    public sealed class PlaylistSong
    {
        public Guid PlaylistId { get; set; }
        public Playlist Playlist { get; set; } = null!;

        public Guid SongId { get; set; }
        public Song Song { get; set; } = null!;

        public int OrderIndex { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
