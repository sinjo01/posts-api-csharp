// Models/Post.cs
// Represents the data structure returned by the JSONPlaceholder /posts API.

namespace PostsApi.Models;

/// <summary>
/// Represents a single blog post from the JSONPlaceholder API.
/// All properties map directly to the JSON field names in the response.
/// </summary>
public class Post
{
    /// <summary>The unique identifier of the post.</summary>
    public int Id { get; set; }

    /// <summary>The ID of the user who created the post.</summary>
    public int UserId { get; set; }

    /// <summary>The title of the post.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>The body/content of the post.</summary>
    public string Body { get; set; } = string.Empty;
}
