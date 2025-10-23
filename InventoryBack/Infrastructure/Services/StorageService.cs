using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace InventoryBack.Infrastructure.Services;

/// <summary>
/// Service for uploading and managing files in Supabase Storage.
/// </summary>
public class StorageService : Application.Services.IStorageService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public StorageService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _httpClient = httpClientFactory?.CreateClient() ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public async Task<string> UploadImageAsync(IFormFile file, string bucket = "products", CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is empty or null.", nameof(file));
        }

        var supabaseUrl = _configuration["Supabase:Url"];
        var supabaseKey = _configuration["Supabase:Key"];

        if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(supabaseKey))
        {
            throw new InvalidOperationException("Supabase configuration is missing.");
        }

        // Generate unique file name
        var fileName = GenerateUniqueFileName(file.FileName);

        // Prepare upload URL
        var uploadUrl = $"{supabaseUrl}/storage/v1/object/{bucket}/{fileName}";

        // Read file content
        using var stream = file.OpenReadStream();
        using var content = new StreamContent(stream);
        content.Headers.Add("Content-Type", file.ContentType);

        // Add authorization header
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseKey}");

        // Upload file
        var response = await _httpClient.PostAsync(uploadUrl, content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new Exception($"Failed to upload image to Supabase: {error}");
        }

        // Return public URL
        var publicUrl = $"{supabaseUrl}/storage/v1/object/public/{bucket}/{fileName}";
        return publicUrl;
    }

    public async Task DeleteImageAsync(string imageUrl, string bucket = "products", CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(imageUrl))
        {
            return; // Nothing to delete
        }

        var supabaseUrl = _configuration["Supabase:Url"];
        var supabaseKey = _configuration["Supabase:Key"];

        if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(supabaseKey))
        {
            throw new InvalidOperationException("Supabase configuration is missing.");
        }

        // Extract file name from URL
        var fileName = Path.GetFileName(new Uri(imageUrl).LocalPath);

        // Prepare delete URL
        var deleteUrl = $"{supabaseUrl}/storage/v1/object/{bucket}/{fileName}";

        // Add authorization header
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseKey}");

        // Delete file
        var response = await _httpClient.DeleteAsync(deleteUrl, ct);

        if (!response.IsSuccessStatusCode)
        {
            // Log warning but don't throw (file might not exist)
            var error = await response.Content.ReadAsStringAsync(ct);
            Console.WriteLine($"Warning: Failed to delete image from Supabase: {error}");
        }
    }

    public string GenerateUniqueFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var guid = Guid.NewGuid().ToString("N")[..8];
        return $"product_{timestamp}_{guid}{extension}";
    }
}
