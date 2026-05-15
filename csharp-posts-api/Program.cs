// Program.cs
// Entry point for the ASP.NET Core Web API.
// Configures services (DI container), the HTTP request pipeline, Swagger UI,
// and the named HttpClient that talks to JSONPlaceholder.

using PostsApi.Services;

// ---------------------------------------------------------------------------
// 1. Create the WebApplication builder
// ---------------------------------------------------------------------------
var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// 2. Configure the server to listen on the port Replit assigns ($PORT),
//    defaulting to 5000 for local development.
// ---------------------------------------------------------------------------
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ---------------------------------------------------------------------------
// 3. Register services in the DI container
// ---------------------------------------------------------------------------

// Add MVC controllers (discovers PostsController automatically)
builder.Services.AddControllers();

// Register the named HttpClient "JsonPlaceholder" with:
//   - A base address pointing to the external API
//   - A 30-second timeout to prevent hanging requests
//   - A default Accept header so the remote API returns JSON
builder.Services.AddHttpClient("JsonPlaceholder", client =>
{
    client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

// Register ApiService as the implementation of IApiService.
// AddScoped means one instance per HTTP request, which is appropriate here
// because ApiService is stateless and holds an HttpClient resolved each time.
builder.Services.AddScoped<IApiService, ApiService>();

// ---------------------------------------------------------------------------
// 4. Configure Swagger / OpenAPI
// ---------------------------------------------------------------------------

// Adds the API explorer that Swagger uses to discover endpoints
builder.Services.AddEndpointsApiExplorer();

// Adds the Swagger document generator with metadata
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Posts API",
        Version = "v1",
        Description = "A beginner-friendly ASP.NET Core Web API that proxies " +
                      "the JSONPlaceholder /posts resource. " +
                      "Use the Swagger UI below to test every endpoint interactively."
    });

    // Include XML comments (controller summaries) in Swagger docs
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// ---------------------------------------------------------------------------
// 5. Build the WebApplication
// ---------------------------------------------------------------------------
var app = builder.Build();

// ---------------------------------------------------------------------------
// 6. Configure the HTTP request pipeline (middleware order matters!)
// ---------------------------------------------------------------------------

// Enable Swagger JSON endpoint and the interactive UI.
// Available at /swagger in both Development and Production so it can be
// tested on Replit's preview URL.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Posts API v1");
    // Serve the Swagger UI at the root path ("/") for convenience.
    options.RoutePrefix = string.Empty;
});

// Redirect HTTP to HTTPS (omitted in dev / Replit to avoid proxy issues)
// app.UseHttpsRedirection();

// Route requests to controller action methods
app.UseAuthorization();
app.MapControllers();

// ---------------------------------------------------------------------------
// 7. Start the application
// ---------------------------------------------------------------------------
app.Run();
