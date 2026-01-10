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