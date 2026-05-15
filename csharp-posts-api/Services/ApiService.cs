// Services/ApiService.cs
// Concrete implementation of IApiService.
// Uses the named HttpClient "JsonPlaceholder" which is registered in Program.cs.

using System.Net;
using System.Net.Http.Json;          // JsonContent, ReadFromJsonAsync
using PostsApi.Models;

namespace PostsApi.Services;

/// <summary>
/// Handles all HTTP communication with the JSONPlaceholder REST API.
/// HttpClient is injected via IHttpClientFactory for safe connection pooling.
/// </summary>
public class ApiService : IApiService
{
    // -------------------------------------------------------------------------
    // Fields
    // -------------------------------------------------------------------------

    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiService> _logger;

    // Base URL for the posts resource (relative to the client BaseAddress)
    private const string PostsEndpoint = "posts";

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Receives the named HttpClient and a logger through dependency injection.
    /// </summary>
    public ApiService(IHttpClientFactory httpClientFactory, ILogger<ApiService> logger)
    {
        // Resolve the named client configured in Program.cs
        _httpClient = httpClientFactory.CreateClient("JsonPlaceholder");
        _logger = logger;
    }

    // -------------------------------------------------------------------------
    // GET – all posts
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<IEnumerable<Post>?> GetAllPostsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all posts from {Endpoint}", PostsEndpoint);

            // ReadFromJsonAsync automatically deserializes the JSON array into
            // a list of Post objects using System.Text.Json.
            var posts = await _httpClient.GetFromJsonAsync<IEnumerable<Post>>(PostsEndpoint);
            return posts;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while fetching all posts");
            throw; // Re-throw so the controller can return the appropriate status code
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Request timed out while fetching all posts");
            throw;
        }
    }

    // -------------------------------------------------------------------------
    // GET – single post
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<Post?> GetPostByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Fetching post with id={Id}", id);

            var response = await _httpClient.GetAsync($"{PostsEndpoint}/{id}");

            // Return null when the resource does not exist so the controller
            // can translate that into a clean 404 response.
            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Post>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while fetching post id={Id}", id);
            throw;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Request timed out while fetching post id={Id}", id);
            throw;
        }
    }

    // -------------------------------------------------------------------------
    // POST – create
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<Post?> CreatePostAsync(Post post)
    {
        try
        {
            _logger.LogInformation("Creating new post with title='{Title}'", post.Title);

            // JsonContent.Create serializes the Post object to JSON and sets
            // the Content-Type header to application/json automatically.
            var response = await _httpClient.PostAsJsonAsync(PostsEndpoint, post);
            response.EnsureSuccessStatusCode();

            // JSONPlaceholder echoes back the created resource (id=101)
            return await response.Content.ReadFromJsonAsync<Post>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while creating a post");
            throw;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Request timed out while creating a post");
            throw;
        }
    }

    // -------------------------------------------------------------------------
    // PUT – update (full replacement)
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<Post?> UpdatePostAsync(int id, Post post)
    {
        try
        {
            _logger.LogInformation("Updating post id={Id}", id);

            var response = await _httpClient.PutAsJsonAsync($"{PostsEndpoint}/{id}", post);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Post>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while updating post id={Id}", id);
            throw;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Request timed out while updating post id={Id}", id);
            throw;
        }
    }

    // -------------------------------------------------------------------------
    // DELETE
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<bool> DeletePostAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deleting post id={Id}", id);

            var response = await _httpClient.DeleteAsync($"{PostsEndpoint}/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound)
                return false;

            response.EnsureSuccessStatusCode();

            // JSONPlaceholder returns 200 with an empty object {} on successful delete
            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while deleting post id={Id}", id);
            throw;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Request timed out while deleting post id={Id}", id);
            throw;
        }
    }
}
