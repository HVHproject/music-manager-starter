using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using music_manager_starter.Shared;
using System.Net.Http.Json;

namespace music_manager_starter.Client.Pages
{
    public partial class Playlists
    {
        private List<PlaylistDto> playlists = new();
        private PlaylistDto? selectedPlaylist;
        private string newPlaylistName = string.Empty;
        private string renamePlaylistName = string.Empty;
        private bool showMenu = false;
        private bool showRenameModal = false;
        private bool showDeleteModal = false;
        private bool showAddSongsModal = false;
        private string searchQuery = string.Empty;
        private List<Song>? searchResults;
        private HashSet<Guid> selectedSongIds = new();
        private bool isSearching = false;
        private bool isLoadingMore = false;
        private bool hasMoreSearchResults = false;
        private Guid? searchCursor = null;
        private bool isBulkDeleteMode = false;
        private HashSet<Guid> bulkDeleteSelectedIds = new();

        private IReadOnlyList<PlaylistSongDto> orderedSongs =>
            selectedPlaylist is null
                ? Array.Empty<PlaylistSongDto>()
                : selectedPlaylist.PlaylistSongs
                    .OrderBy(ps => ps.OrderIndex)
                    .ToList();

        protected override async Task OnInitializedAsync()
        {
            await LoadPlaylists();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JS.InvokeVoidAsync("eval", @"
                document.addEventListener('click', function() {
                    var component = document.querySelector('[data-playlist-component]');
                    if (component) {
                        component.__instance.invokeMethodAsync('CloseMenu');
                    }
                });
            ");
            }
        }

        private async Task LoadPlaylists()
        {
            playlists = await Http.GetFromJsonAsync<List<PlaylistDto>>("api/playlists") ?? new();
        }

        private async Task CreatePlaylist()
        {
            if (string.IsNullOrWhiteSpace(newPlaylistName))
                return;

            var playlistName = newPlaylistName;

            // Optimistic: Create temporary playlist immediately
            var tempId = Guid.NewGuid();
            var tempPlaylist = new PlaylistDto
            {
                Id = tempId,
                Name = playlistName,
                CreatedAt = DateTime.Now,
                PlaylistSongs = new List<PlaylistSongDto>()
            };

            playlists.Insert(0, tempPlaylist);
            newPlaylistName = string.Empty;
            StateHasChanged();

            try
            {
                var response = await Http.PostAsJsonAsync("api/playlists", playlistName);

                if (response.IsSuccessStatusCode)
                {
                    var realId = await response.Content.ReadFromJsonAsync<Guid>();

                    // Replace temp playlist with one that has the real ID
                    var index = playlists.FindIndex(p => p.Id == tempId);
                    if (index >= 0)
                    {
                        playlists[index] = new PlaylistDto
                        {
                            Id = realId,
                            Name = playlistName,
                            CreatedAt = tempPlaylist.CreatedAt,
                            PlaylistSongs = new List<PlaylistSongDto>()
                        };
                    }
                    StateHasChanged();
                }
                else
                {
                    // Rollback: Remove the temporary playlist
                    playlists.RemoveAll(p => p.Id == tempId);
                    newPlaylistName = playlistName;
                    StateHasChanged();
                }
            }
            catch
            {
                // Rollback on error
                playlists.RemoveAll(p => p.Id == tempId);
                newPlaylistName = playlistName;
                StateHasChanged();
            }
        }

        private async Task SelectPlaylist(PlaylistDto playlist)
        {
            selectedPlaylist = await Http.GetFromJsonAsync<PlaylistDto>($"api/playlists/{playlist.Id}");
            showMenu = false;
            StateHasChanged();
        }

        private void ShowRenameModal()
        {
            if (selectedPlaylist is not null)
            {
                renamePlaylistName = selectedPlaylist.Name;
                showRenameModal = true;
            }
        }

        private void ShowDeleteModal()
        {
            showDeleteModal = true;
        }

        private void ShowAddSongsModal()
        {
            showAddSongsModal = true;
            ResetAddSongsModalState();
        }

        private void ResetAddSongsModalState()
        {
            searchQuery = string.Empty;
            selectedSongIds.Clear();
            searchResults = null;
            searchCursor = null;
            hasMoreSearchResults = false;
            isSearching = false;
            isLoadingMore = false;
        }

        private async Task SearchSongs()
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
                return;

            isSearching = true;
            searchResults = null;
            searchCursor = null;
            hasMoreSearchResults = false;
            StateHasChanged();

            try
            {
                var queryParams = new List<string>();

                if (!string.IsNullOrWhiteSpace(searchQuery))
                    queryParams.Add($"query={Uri.EscapeDataString(searchQuery)}");

                var apiUrl = "api/songs/search";
                if (queryParams.Any())
                    apiUrl += "?" + string.Join("&", queryParams);

                var response = await Http.GetFromJsonAsync<SongSearchResponse>(apiUrl);
                searchResults = response?.Results ?? new List<Song>();
                searchCursor = response?.NextCursor;
                hasMoreSearchResults = searchCursor.HasValue;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Search error: {ex.Message}");
                searchResults = new List<Song>();
            }
            finally
            {
                isSearching = false;
                StateHasChanged();
            }
        }

        private async Task LoadMoreSearchResults()
        {
            if (!hasMoreSearchResults || isLoadingMore || searchCursor == null)
                return;

            isLoadingMore = true;
            StateHasChanged();

            try
            {
                var queryParams = new List<string>();

                if (!string.IsNullOrWhiteSpace(searchQuery))
                    queryParams.Add($"query={Uri.EscapeDataString(searchQuery)}");

                queryParams.Add($"cursor={searchCursor}");

                var apiUrl = "api/songs/search?" + string.Join("&", queryParams);
                var response = await Http.GetFromJsonAsync<SongSearchResponse>(apiUrl);

                if (response != null)
                {
                    searchResults?.AddRange(response.Results);
                    searchCursor = response.NextCursor;
                    hasMoreSearchResults = searchCursor.HasValue;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Load more error: {ex.Message}");
            }
            finally
            {
                isLoadingMore = false;
                StateHasChanged();
            }
        }

        private void ToggleSongSelection(Guid songId)
        {
            if (selectedSongIds.Contains(songId))
                selectedSongIds.Remove(songId);
            else
                selectedSongIds.Add(songId);
        }

        private void ClearSelection()
        {
            selectedSongIds.Clear();
        }

        private async Task AddSelectedSongs()
        {
            if (selectedPlaylist is null || !selectedSongIds.Any())
                return;

            var songsToAdd = selectedSongIds.ToList();

            // Store backup
            var backupSongs = selectedPlaylist.PlaylistSongs.ToList();
            var backupInList = playlists.FirstOrDefault(p => p.Id == selectedPlaylist.Id);
            var backupListSongs = backupInList?.PlaylistSongs.ToList();

            // Optimistic: Add songs immediately with temporary data
            var nextOrderIndex = selectedPlaylist.PlaylistSongs.Any()
                ? selectedPlaylist.PlaylistSongs.Max(ps => ps.OrderIndex) + 1
                : 0;

            var tempPlaylistSongs = new List<PlaylistSongDto>();
            foreach (var songId in songsToAdd)
            {
                var song = searchResults?.FirstOrDefault(s => s.Id == songId);
                if (song != null)
                {
                    tempPlaylistSongs.Add(new PlaylistSongDto
                    {
                        PlaylistId = selectedPlaylist.Id,
                        SongId = songId,
                        OrderIndex = nextOrderIndex++,
                        AddedAt = DateTime.Now,
                        Song = song
                    });
                }
            }

            var newSongsList = selectedPlaylist.PlaylistSongs.ToList();
            newSongsList.AddRange(tempPlaylistSongs);
            selectedPlaylist.PlaylistSongs = newSongsList;

            if (backupInList != null)
            {
                backupInList.PlaylistSongs = newSongsList.ToList();
            }

            CloseModals();
            StateHasChanged();

            try
            {
                var response = await Http.PostAsJsonAsync(
                    $"api/playlists/{selectedPlaylist.Id}/songs",
                    songsToAdd);

                if (response.IsSuccessStatusCode)
                {
                    var refreshed = await Http.GetFromJsonAsync<PlaylistDto>($"api/playlists/{selectedPlaylist.Id}");
                    if (refreshed != null)
                    {
                        selectedPlaylist = refreshed;

                        var playlistInList = playlists.FirstOrDefault(p => p.Id == selectedPlaylist.Id);
                        if (playlistInList != null)
                        {
                            playlistInList.PlaylistSongs = refreshed.PlaylistSongs.ToList();
                        }
                    }
                    StateHasChanged();
                }
                else
                {
                    // Rollback: Remove the temporary songs
                    selectedPlaylist.PlaylistSongs = backupSongs;
                    if (backupInList != null && backupListSongs != null)
                    {
                        backupInList.PlaylistSongs = backupListSongs;
                    }
                    StateHasChanged();
                }
            }
            catch
            {
                // Rollback on error
                selectedPlaylist.PlaylistSongs = backupSongs;
                if (backupInList != null && backupListSongs != null)
                {
                    backupInList.PlaylistSongs = backupListSongs;
                }
                StateHasChanged();
            }
        }

        private async Task RenamePlaylist()
        {
            if (selectedPlaylist is null || string.IsNullOrWhiteSpace(renamePlaylistName))
                return;

            var newName = renamePlaylistName;
            var oldName = selectedPlaylist.Name;

            // Optimistic: Update name immediately
            selectedPlaylist.Name = newName;
            var playlistInList = playlists.FirstOrDefault(p => p.Id == selectedPlaylist.Id);
            if (playlistInList != null)
            {
                playlistInList.Name = newName;
            }

            showRenameModal = false;
            renamePlaylistName = string.Empty;
            StateHasChanged();

            try
            {
                var response = await Http.PutAsJsonAsync($"api/playlists/{selectedPlaylist.Id}/rename", newName);

                if (!response.IsSuccessStatusCode)
                {
                    // Rollback: Restore old name
                    selectedPlaylist.Name = oldName;
                    if (playlistInList != null)
                    {
                        playlistInList.Name = oldName;
                    }
                    StateHasChanged();
                }
            }
            catch
            {
                // Rollback on error
                selectedPlaylist.Name = oldName;
                if (playlistInList != null)
                {
                    playlistInList.Name = oldName;
                }
                StateHasChanged();
            }
        }

        private async Task DeletePlaylist()
        {
            if (selectedPlaylist is null)
                return;

            var playlistToDelete = selectedPlaylist;
            var playlistIndex = playlists.FindIndex(p => p.Id == playlistToDelete.Id);

            // Optimistic: Remove immediately
            playlists.RemoveAll(p => p.Id == playlistToDelete.Id);
            showDeleteModal = false;
            selectedPlaylist = null;
            StateHasChanged();

            try
            {
                var response = await Http.DeleteAsync($"api/playlists/{playlistToDelete.Id}");

                if (!response.IsSuccessStatusCode)
                {
                    // Rollback: Restore the playlist
                    if (playlistIndex >= 0 && playlistIndex <= playlists.Count)
                    {
                        playlists.Insert(playlistIndex, playlistToDelete);
                    }
                    else
                    {
                        playlists.Add(playlistToDelete);
                    }
                    StateHasChanged();
                }
            }
            catch
            {
                // Rollback on error
                if (playlistIndex >= 0 && playlistIndex <= playlists.Count)
                {
                    playlists.Insert(playlistIndex, playlistToDelete);
                }
                else
                {
                    playlists.Add(playlistToDelete);
                }
                StateHasChanged();
            }
        }

        private void CloseModals()
        {
            showRenameModal = false;
            showDeleteModal = false;
            showAddSongsModal = false;
            ResetAddSongsModalState();
            renamePlaylistName = string.Empty;
        }

        private void CloseMenu()
        {
            showMenu = false;
        }

        private void ToggleBulkDeleteMode()
        {
            isBulkDeleteMode = !isBulkDeleteMode;
            if (!isBulkDeleteMode)
            {
                ClearBulkDeleteSelection();
            }
        }

        private void ToggleBulkDeleteSelection(Guid songId)
        {
            if (bulkDeleteSelectedIds.Contains(songId))
                bulkDeleteSelectedIds.Remove(songId);
            else
                bulkDeleteSelectedIds.Add(songId);
        }

        private void SelectAllSongsForBulkDelete()
        {
            if (selectedPlaylist?.PlaylistSongs != null)
            {
                bulkDeleteSelectedIds = new HashSet<Guid>(
                    selectedPlaylist.PlaylistSongs.Select(ps => ps.SongId)
                );
            }
        }

        private void ClearBulkDeleteSelection()
        {
            bulkDeleteSelectedIds.Clear();
        }

        private void CancelBulkDelete()
        {
            isBulkDeleteMode = false;
            ClearBulkDeleteSelection();
        }

        private async Task ConfirmBulkDelete()
        {
            if (selectedPlaylist is null || bulkDeleteSelectedIds.Count == 0)
                return;

            var songIdsToDelete = bulkDeleteSelectedIds.ToList();

            // Store backup
            var backupSongs = selectedPlaylist.PlaylistSongs.ToList();
            var backupInList = playlists.FirstOrDefault(p => p.Id == selectedPlaylist.Id);
            var backupListSongs = backupInList?.PlaylistSongs.ToList();

            // Optimistic: Remove songs immediately and reorder
            selectedPlaylist.PlaylistSongs = selectedPlaylist.PlaylistSongs
                .Where(ps => !songIdsToDelete.Contains(ps.SongId))
                .OrderBy(ps => ps.OrderIndex)
                .Select((ps, index) => new PlaylistSongDto
                {
                    PlaylistId = ps.PlaylistId,
                    SongId = ps.SongId,
                    OrderIndex = index,
                    AddedAt = ps.AddedAt,
                    Song = ps.Song
                })
                .ToList();

            if (backupInList != null)
            {
                backupInList.PlaylistSongs = selectedPlaylist.PlaylistSongs.ToList();
            }

            isBulkDeleteMode = false;
            ClearBulkDeleteSelection();
            StateHasChanged();

            try
            {
                var response = await Http.SendAsync(new HttpRequestMessage
                {
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri($"api/playlists/{selectedPlaylist.Id}/songs", UriKind.Relative),
                    Content = JsonContent.Create(songIdsToDelete)
                });

                if (response.IsSuccessStatusCode)
                {
                    // Refresh to ensure sync
                    var refreshed = await Http.GetFromJsonAsync<PlaylistDto>($"api/playlists/{selectedPlaylist.Id}");
                    if (refreshed != null)
                    {
                        selectedPlaylist = refreshed;

                        // Update in list
                        var playlistInList = playlists.FirstOrDefault(p => p.Id == selectedPlaylist.Id);
                        if (playlistInList != null)
                        {
                            playlistInList.PlaylistSongs = refreshed.PlaylistSongs.ToList();
                        }
                    }
                    StateHasChanged();
                }
                else
                {
                    // Rollback
                    selectedPlaylist.PlaylistSongs = backupSongs;
                    if (backupInList != null && backupListSongs != null)
                    {
                        backupInList.PlaylistSongs = backupListSongs;
                    }
                    StateHasChanged();
                }
            }
            catch
            {
                // Rollback on error
                selectedPlaylist.PlaylistSongs = backupSongs;
                if (backupInList != null && backupListSongs != null)
                {
                    backupInList.PlaylistSongs = backupListSongs;
                }
                StateHasChanged();
            }
        }

        private async Task ExportSelectedPlaylistCsv()
        {
            if (selectedPlaylist is null)
                return;

            showMenu = false;

            var csv = await Http.GetStringAsync(
                $"api/playlists/{selectedPlaylist.Id}/export?format=csv");

            var fileName =
                $"{SanitizeFileName(selectedPlaylist.Name)}.csv";

            await JS.InvokeVoidAsync(
                "downloadFile",
                fileName,
                "text/csv",
                csv);
        }

        private static string SanitizeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }

            return name;
        }

    }
}