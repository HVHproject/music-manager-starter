# Getting Started #

This solution contains an easy to use and simple music manager.

## Technologies
- Visual Studio 2022 
- .NET 8 SDK
- Node.js (for Tailwind CSS)
- Git
- EntityFramework Core 
- Blazor


## Local Enviroment Setup
You need to have the .NET 8 SDK installed. Be sure to download the latest version for Visual Studio 2022.

Use the latest version of Visual Studio 2022: https://visualstudio.microsoft.com/downloads/

Install the node packages before building the solution by using ```npm install``

Make sure server is the default start up item, then run it in Visual Studio.

Run ```dotnet build``` to test if the project builds.

Run ```dotnet test``` to test the project entirely.


# Song Star Ratings Update

## Architecture Decisions

### Pattern Selection
- **Service Layer Pattern**: Chosen over Repository Pattern for simplicity
- **Dependency Injection**: All services injected via constructor for testability
- **DTO Pattern**: Separate data transfer objects for API responses
- **Middleware**: FakeUserMiddleware for authentication simulation during development

## Features Implemented

### 1. Song Rating System
- Star-based rating component (0-5 stars in 0.5 increments)
- User-specific ratings (simulated via FakeUserMiddleware)
- Rating summary with average and distribution
- Update existing ratings

### 2. Analytics Dashboard
- Most rated songs display
- Rating trends over time
- Genre popularity analysis
- Overall analytics summary

### 3. User Simulation
- FakeUserMiddleware for user context
- Header-based user identification (X-Test-User)
- Default user fallback

## Known Limitations & Future Improvements

### Current Limitations
1. **Fake Authentication**: Uses middleware instead of real auth system
2. **Mock Analytics Data**: Analytics service returns generated mock data
3. **No Real User Management**: Single simulated user context
4. **Basic Error Handling**: Minimal validation and error responses

### Future Improvements
1. **Real Authentication**: Integrate ASP.NET Core Identity
2. **Caching**: Implement caching for frequently accessed data




# Advanced Search Update

## Architecture Decisions

### Search Architecture Pattern
- **Controller Layer**: Thin controller handling HTTP requests/responses, extracts user ID from authentication for personalized results, uses `[FromQuery]` attribute for clean parameter binding
- **Service Layer**: Contains business logic for search operations, handles complex filtering, rating calculations, and pagination, decouples data access from API endpoints
- **Data Access Layer**: Uses EF Core with `AsNoTracking()` for read-only operations, implements efficient rating aggregation using database grouping

### Trade Offs
- **Rating Filtering Implementation**: Two-phase query approach:
1. First query calculates song IDs meeting rating criteria using database grouping
2. Second query applies these IDs along with other filters
Trade off: Slightly more complex but avoids expensive correlated subqueries
- **Frontend State Management**: URL-driven state management
1. Search parameters encoded in URL query string
2. Automatic state restoration on page refresh
3. Shareable search results via URL
Trade off: More complex parsing logic but enables bookmarkable searches

## Features Implemented

### 1. Comprehensive Search Functionality Service
- Searches across title, artist, and album fields (case-insensitive)
- Filters: Genre, Year Range (min/max), Minimum Rating (0.5-5.0 in 0.5 increments)

### 2. Search Results Page
- Search in the overlay from any location on the website
- Use provided dropdowns and inputs for filtering
- Save searches by url
- Pagination via cursor

## Known Limitations & Future Improvements

### Current Limitations
1. **Genre Hardcoding**: Genres are hardcoded in the frontend
2. **Search Functionality**: The search is a simple 'like' query

### Future Improvements
1. **Genre Retrieval**: Should be dynamically fetched from database/API
2. **Tailored Search**: Given more details of the client's needs, a search designed around said needs



# Playlist CRUD Update

## Architecture Decisions

### Layered Architecture Pattern
- **Controller Layer**: Thin REST API controllers handling HTTP endpoints, user authentication extraction, and route parameter binding
- **Service Layer**: Business logic for playlist operations, validation, and data transformation between domain models and DTOs
- **Repository Layer**: Data access abstraction with optimized Entity Framework queries using dictionaries and HashSets for O(n) performance
- **Client-Side State Management**: Playlist management with optimistic UI updates containing rollback mechanisms, client-side undo/redo stacks (max 5 operations), per-playlist state isolation

### Trade-Offs

- **Client-Side vs Server-Side Undo/Redo**
1. Client-side undo/redo implementation using JavaScript stacks
Trade off: Undo history is lost on page refresh and limited to 5 steps, but provides immediate feedback and reduces server load.

- **Prevented Duplicate Additions**
1. Instead of allowing duplicates, and creating a function dedicated to deleting duplicates, I created a system which prevents the addition of duplicates to a playlist.
Trade off: Users cannot add the same song to a playlist more than once, which negates the need to delete duplicates, however barriers user experience if they want to add duplicate songs to a playlist.

## Features Implemented

### 1. Full CRUD Operations
- Create new playlists with user-specific ownership
- Read all playlists for current user with song details
- Update playlist names
- Delete playlists with cascade deletion of songs
- Optimistic UI updates with automatic rollback on failure

### 2. Drag-and-Drop Reordering
- Sortable.js integration via JavaScript interop
- Visual drag handles with smooth animations
- Server-side persistence of new song order
- Automatic DOM synchronization after server confirmation
- Disabled during bulk delete mode

### 3. Bulk Operations
- Add multiple songs at once from search results
- Select songs with checkboxes in add modal
- Bulk delete mode with multi-select
- "Select All" and "Clear Selection" shortcuts
- Visual feedback showing selection count

### 4. Export Functionality
- Export to CSV format with proper escaping
- Export to JSON format with indentation
- Includes song metadata (title, artist, album, genre, year)
- Sanitized filenames for safe downloads

### 5. Undo/Redo System
- Client-side stack-based implementation
- Supports drag-and-drop reordering, adding songs, and removing songs
- Maximum 5 undo steps per playlist
- Visual feedback (buttons enabled/disabled based on stack state)
- Automatic state clearing when switching playlists
- Synchronized server updates on undo/redo

## Known Limitations & Future Improvements

### Current Limitations
1. **Undo/Redo Scope**: Limited to 5 operations per playlist, history lost on page refresh, not stored server side
2. **Bulk Operations**: Cannot add songs that already exist in playlist (skipped silently), no duplicate detection warnings

### Future Improvements

1. **Enhanced Undo/Redo**: Depending on the client's needs, an expansion or even a refactor may be in order. If asked, it could hold history between playlists and even pages and increase history depth on expected needs.
2. **Export/Import Features**: Personalized export for personal use, choosing which fields to export. Json export for potential import feature.
3. **User Experience**: Playlist folders/categories/search, add songs from global advanced search, add search inside playlists.
4. **Duplicate Songs**: Depending on the client's needs, duplicate songs could be handled in the following ways: enabled with a function for mass deletion, marked in the addition menu as already added, trigger a toast stating certain songs are already in the playlist.