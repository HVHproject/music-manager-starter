namespace music_manager_starter.Shared
{
    public sealed class SongSearchRequest
    {
        public string? Query { get; set; }

        public string? Genre { get; set; }

        public int? MinYear { get; set; }
        public int? MaxYear { get; set; }

        public double? MinRating { get; set; }

        /// <summary>
        /// Cursor for keyset pagination
        /// </summary>
        public Guid? Cursor { get; set; }

        public int PageSize { get; set; } = 8;
    }

    public sealed class SongSearchResponse
    {
        public List<Song> Results { get; set; } = new();

        public Guid? NextCursor { get; set; }
    }
}
