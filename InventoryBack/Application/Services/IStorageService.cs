using Microsoft.AspNetCore.Http;

namespace InventoryBack.Application.Services;

/// <summary>
/// Service interface for uploading and managing files in Supabase Storage.
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Uploads an image to Supabase Storage.
    /// </summary>
    /// <param name="file">Image file</param>
    /// <param name="bucket">Storage bucket name (e.g., "products")</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Public URL of the uploaded image</returns>
    Task<string> UploadImageAsync(IFormFile file, string bucket = "products", CancellationToken ct = default);

    /// <summary>
    /// Deletes an image from Supabase Storage.
    /// </summary>
    /// <param name="imageUrl">Public URL of the image</param>
    /// <param name="bucket">Storage bucket name</param>
    /// <param name="ct">Cancellation token</param>
    Task DeleteImageAsync(string imageUrl, string bucket = "products", CancellationToken ct = default);

    /// <summary>
    /// Generates a unique file name for the image.
    /// </summary>
    /// <param name="originalFileName">Original file name</param>
    /// <returns>Unique file name</returns>
    string GenerateUniqueFileName(string originalFileName);
}
