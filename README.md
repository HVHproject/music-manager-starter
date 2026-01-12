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

