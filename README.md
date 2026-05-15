# Posts API — ASP.NET Core Web API (C#)

A beginner-friendly ASP.NET Core 8 Web API that proxies the JSONPlaceholder `/posts` REST API. Includes full Swagger UI for interactive testing.

## Run & Operate

- Workflow **"Posts API (C#)"** starts automatically — visit the webview to open Swagger UI.
- To run manually: `cd csharp-posts-api && dotnet run`
- To build: `cd csharp-posts-api && dotnet build`
- To restore packages: `cd csharp-posts-api && dotnet restore`

## Stack

- .NET 8 (LTS), C#, ASP.NET Core Web API
- HttpClient (IHttpClientFactory, named client)
- Swagger via Swashbuckle.AspNetCore 6.6.2
- Dependency Injection (built-in ASP.NET Core DI)
- System.Text.Json for serialization/deserialization
- Async/Await throughout

## Where things live

```
csharp-posts-api/
├── PostsApi.csproj           — Project file, NuGet references
├── Program.cs                — Entry point, DI registration, Swagger setup
├── Models/
│   └── Post.cs               — Post data model (Id, UserId, Title, Body)
├── Services/
│   ├── IApiService.cs        — Interface (contract) for all post operations
│   └── ApiService.cs         — HttpClient implementation of IApiService
└── Controllers/
    └── PostsController.cs    — 5 REST endpoints with error handling
```

## API Endpoints

| Method | Route            | Description              |
|--------|------------------|--------------------------|
| GET    | /api/posts       | Retrieve all posts       |
| GET    | /api/posts/{id}  | Retrieve post by ID      |
| POST   | /api/posts       | Create a new post        |
| PUT    | /api/posts/{id}  | Update an existing post  |
| DELETE | /api/posts/{id}  | Delete a post            |

External API: `https://jsonplaceholder.typicode.com/posts`

## Architecture decisions

- **Named HttpClient via IHttpClientFactory** — avoids socket exhaustion from creating new HttpClient instances per request; timeout configured at 30 s.
- **IApiService interface** — decouples the controller from the HTTP implementation; makes the service swappable for tests (mock) or a different backend.
- **Scoped service lifetime** — ApiService is stateless so one instance per HTTP request is correct and safe.
- **Swagger at root path** — `RoutePrefix = string.Empty` places Swagger UI at `/` for easy discovery in Replit's webview.
- **IActionResult returns** — each endpoint picks its own status code (200/201/204/400/404/502/504/500) instead of throwing globally.

## Error handling

- 400 — invalid ID (≤ 0) or missing required fields
- 404 — upstream returned 404 (post not found)
- 502 — HttpRequestException from upstream
- 504 — TaskCanceledException / timeout (30 s limit)
- 500 — unexpected exceptions

## User preferences

- Project should be production-style and beginner friendly.
- Full comments explaining each code decision.
- All C# specific to `csharp-posts-api/` directory.

## Gotchas

- The C# app listens on `$PORT` (defaults to 5000). Replit's shared reverse proxy routes `/api` to the Node.js API Server artifact; access the C# app through the **"Posts API (C#)"** workflow webview, not through `/api` on the shared proxy.
- Run `dotnet restore` before `dotnet build` after cloning on a new machine.
