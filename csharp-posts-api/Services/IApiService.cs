// Services/IApiService.cs
// Interface that defines all post-related operations.
// Using an interface enables dependency injection and makes the service
// easy to unit-test by swapping with a mock implementation.

using PostsApi.Models;

namespace PostsApi.Services;

/// <summary>
/// Contract for all operations against the JSONPlaceholder /posts API.
/// </summary>
public interface IApiService
{
    /// <summary>Retrieve all posts.</summary>
    Task<IEnumerable<Post>?> GetAllPostsAsync();

    /// <summary>Retrieve a single post by its ID.</summary>
    Task<Post?> GetPostByIdAsync(int id);

    /// <summary>Create a new post and return the server response.</summary>
    Task<Post?> CreatePostAsync(Post post);

    /// <summary>Replace an existing post and return the updated version.</summary>
    Task<Post?> UpdatePostAsync(int id, Post post);

    /// <summary>Delete a post. Returns true if the server reported success.</summary>
    Task<bool> DeletePostAsync(int id);
}
