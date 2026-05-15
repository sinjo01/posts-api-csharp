// Controllers/PostsController.cs
// Exposes the five REST endpoints the user requested and converts service
// responses / exceptions into appropriate HTTP status codes.

using Microsoft.AspNetCore.Mvc;
using PostsApi.Models;
using PostsApi.Services;

namespace PostsApi.Controllers;

/// <summary>
/// CRUD endpoints for blog posts, backed by the JSONPlaceholder external API.
/// All methods are async and return IActionResult so each can choose its own
/// HTTP status code.
/// </summary>
[ApiController]
[Route("api/posts")]         // Base route: /api/posts
[Produces("application/json")]
public class PostsController : ControllerBase
{
    // -------------------------------------------------------------------------
    // Fields
    // -------------------------------------------------------------------------

    private readonly IApiService _apiService;
    private readonly ILogger<PostsController> _logger;

    // -------------------------------------------------------------------------
    // Constructor – IApiService is injected by the DI container
    // -------------------------------------------------------------------------

    public PostsController(IApiService apiService, ILogger<PostsController> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    // -------------------------------------------------------------------------
    // GET /api/posts
    // Returns all posts (up to 100 from JSONPlaceholder)
    // -------------------------------------------------------------------------

    /// <summary>Retrieve all posts.</summary>
    /// <response code="200">List of all posts.</response>
    /// <response code="500">Upstream API error or timeout.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Post>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllPosts()
    {
        try
        {
            var posts = await _apiService.GetAllPostsAsync();
            return Ok(posts);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Upstream API error in GetAllPosts");
            return StatusCode(StatusCodes.Status502BadGateway,
                new { error = "Failed to reach the upstream API. Please try again later." });
        }
        catch (TaskCanceledException)
        {
            return StatusCode(StatusCodes.Status504GatewayTimeout,
                new { error = "The upstream API did not respond in time." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetAllPosts");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "An unexpected error occurred." });
        }
    }

    // -------------------------------------------------------------------------
    // GET /api/posts/{id}
    // Returns a single post or 404 if it does not exist
    // -------------------------------------------------------------------------

    /// <summary>Retrieve a single post by ID.</summary>
    /// <param name="id">The post ID (1–100 on JSONPlaceholder).</param>
    /// <response code="200">The requested post.</response>
    /// <response code="400">ID is out of valid range.</response>
    /// <response code="404">Post not found.</response>
    /// <response code="500">Upstream API error or timeout.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Post), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPostById(int id)
    {
        // Validate input – IDs must be positive integers
        if (id <= 0)
            return BadRequest(new { error = "Post ID must be a positive integer." });

        try
        {
            var post = await _apiService.GetPostByIdAsync(id);

            if (post is null)
                return NotFound(new { error = $"Post with ID {id} was not found." });

            return Ok(post);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Upstream API error in GetPostById id={Id}", id);
            return StatusCode(StatusCodes.Status502BadGateway,
                new { error = "Failed to reach the upstream API. Please try again later." });
        }
        catch (TaskCanceledException)
        {
            return StatusCode(StatusCodes.Status504GatewayTimeout,
                new { error = "The upstream API did not respond in time." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetPostById id={Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "An unexpected error occurred." });
        }
    }

    // -------------------------------------------------------------------------
    // POST /api/posts
    // Creates a new post and returns 201 Created with the new resource
    // -------------------------------------------------------------------------

    /// <summary>Create a new post.</summary>
    /// <param name="post">Post data to create. <c>Id</c> is ignored by the API.</param>
    /// <response code="201">The created post (JSONPlaceholder assigns id=101).</response>
    /// <response code="400">Request body is null or required fields are missing.</response>
    /// <response code="500">Upstream API error or timeout.</response>
    [HttpPost]
    [ProducesResponseType(typeof(Post), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreatePost([FromBody] Post post)
    {
        // [ApiController] already validates ModelState, but we add explicit
        // null/content checks for a friendlier developer experience.
        if (post is null)
            return BadRequest(new { error = "Request body cannot be null." });

        if (string.IsNullOrWhiteSpace(post.Title))
            return BadRequest(new { error = "Title is required." });

        if (string.IsNullOrWhiteSpace(post.Body))
            return BadRequest(new { error = "Body is required." });

        try
        {
            var created = await _apiService.CreatePostAsync(post);

            // 201 Created with a Location header pointing to the new resource
            return CreatedAtAction(nameof(GetPostById), new { id = created?.Id }, created);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Upstream API error in CreatePost");
            return StatusCode(StatusCodes.Status502BadGateway,
                new { error = "Failed to reach the upstream API. Please try again later." });
        }
        catch (TaskCanceledException)
        {
            return StatusCode(StatusCodes.Status504GatewayTimeout,
                new { error = "The upstream API did not respond in time." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in CreatePost");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "An unexpected error occurred." });
        }
    }

    // -------------------------------------------------------------------------
    // PUT /api/posts/{id}
    // Replaces an existing post (full update)
    // -------------------------------------------------------------------------

    /// <summary>Update (replace) an existing post.</summary>
    /// <param name="id">ID of the post to update.</param>
    /// <param name="post">Replacement post data.</param>
    /// <response code="200">The updated post.</response>
    /// <response code="400">Invalid ID or missing required fields.</response>
    /// <response code="404">Post not found.</response>
    /// <response code="500">Upstream API error or timeout.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(Post), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdatePost(int id, [FromBody] Post post)
    {
        if (id <= 0)
            return BadRequest(new { error = "Post ID must be a positive integer." });

        if (post is null)
            return BadRequest(new { error = "Request body cannot be null." });

        if (string.IsNullOrWhiteSpace(post.Title))
            return BadRequest(new { error = "Title is required." });

        if (string.IsNullOrWhiteSpace(post.Body))
            return BadRequest(new { error = "Body is required." });

        try
        {
            var updated = await _apiService.UpdatePostAsync(id, post);

            if (updated is null)
                return NotFound(new { error = $"Post with ID {id} was not found." });

            return Ok(updated);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Upstream API error in UpdatePost id={Id}", id);
            return StatusCode(StatusCodes.Status502BadGateway,
                new { error = "Failed to reach the upstream API. Please try again later." });
        }
        catch (TaskCanceledException)
        {
            return StatusCode(StatusCodes.Status504GatewayTimeout,
                new { error = "The upstream API did not respond in time." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in UpdatePost id={Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "An unexpected error occurred." });
        }
    }

    // -------------------------------------------------------------------------
    // DELETE /api/posts/{id}
    // Deletes a post and returns 204 No Content on success
    // -------------------------------------------------------------------------

    /// <summary>Delete a post by ID.</summary>
    /// <param name="id">ID of the post to delete.</param>
    /// <response code="204">Post deleted successfully.</response>
    /// <response code="400">ID is invalid.</response>
    /// <response code="404">Post not found.</response>
    /// <response code="500">Upstream API error or timeout.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeletePost(int id)
    {
        if (id <= 0)
            return BadRequest(new { error = "Post ID must be a positive integer." });

        try
        {
            var deleted = await _apiService.DeletePostAsync(id);

            if (!deleted)
                return NotFound(new { error = $"Post with ID {id} was not found." });

            // 204 No Content is the standard success response for DELETE
            return NoContent();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Upstream API error in DeletePost id={Id}", id);
            return StatusCode(StatusCodes.Status502BadGateway,
                new { error = "Failed to reach the upstream API. Please try again later." });
        }
        catch (TaskCanceledException)
        {
            return StatusCode(StatusCodes.Status504GatewayTimeout,
                new { error = "The upstream API did not respond in time." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in DeletePost id={Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "An unexpected error occurred." });
        }
    }
}
