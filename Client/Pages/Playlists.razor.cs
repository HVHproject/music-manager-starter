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

            var response = await Http.PostAsJsonAsync("api/playlists", newPlaylistName);

            if (response.IsSuccessStatusCode)
            {
                newPlaylistName = string.Empty;
                await LoadPlaylists();
            }
        }

        private async Task SelectPlaylist(PlaylistDto playlist)
        {
            selectedPlaylist = await Http.GetFromJsonAsync<PlaylistDto>($"api/playlists/{playlist.Id}");
            showMenu = false;
        }

        private async Task RemoveSong(Guid songId)
        {
            if (selectedPlaylist is null)
                return;

            await Http.SendAsync(new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri($"api/playlists/{selectedPlaylist.Id}/songs", UriKind.Relative),
                Content = JsonContent.Create(new[] { songId })
            });

            await SelectPlaylist(selectedPlaylist);
        }

        private async Task RemoveAllSongs()
        {
            if (selectedPlaylist is null || !selectedPlaylist.PlaylistSongs.Any())
                return;

            var songIds = selectedPlaylist.PlaylistSongs.Select(ps => ps.SongId).ToList();

            await Http.SendAsync(new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri($"api/playlists/{selectedPlaylist.Id}/songs", UriKind.Relative),
                Content = JsonContent.Create(songIds)
            });

            await SelectPlaylist(selectedPlaylist);
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

            try
            {
                var response = await Http.PostAsJsonAsync(
                    $"api/playlists/{selectedPlaylist.Id}/songs",
                    selectedSongIds.ToList());

                if (response.IsSuccessStatusCode)
                {
                    await SelectPlaylist(selectedPlaylist);
                    CloseModals();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Add songs error: {ex.Message}");
            }
        }


        private async Task RenamePlaylist()
        {
            if (selectedPlaylist is null || string.IsNullOrWhiteSpace(renamePlaylistName))
                return;

            await Http.PutAsJsonAsync($"api/playlists/{selectedPlaylist.Id}/rename", renamePlaylistName);

            showRenameModal = false;
            renamePlaylistName = string.Empty;
            await LoadPlaylists();

            // Refresh the selected playlist
            if (selectedPlaylist is not null)
            {
                await SelectPlaylist(selectedPlaylist);
            }
        }

        private async Task DeletePlaylist()
        {
            if (selectedPlaylist is null)
                return;

            var response = await Http.DeleteAsync($"api/playlists/{selectedPlaylist.Id}");

            if (response.IsSuccessStatusCode)
            {
                showDeleteModal = false;
                selectedPlaylist = null;
                await LoadPlaylists();
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

            await Http.SendAsync(new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri($"api/playlists/{selectedPlaylist.Id}/songs", UriKind.Relative),
                Content = JsonContent.Create(bulkDeleteSelectedIds.ToList())
            });

            await SelectPlaylist(selectedPlaylist);

            isBulkDeleteMode = false;
            ClearBulkDeleteSelection();
        }
    }
}