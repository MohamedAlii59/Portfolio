using Microsoft.AspNetCore.Http;
namespace Portfolio.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(IFormFile file, string folder, string[] allowedContentTypes, long maxSizeBytes);
        Task DeleteFileAsync(string? storedUrl);
        Task<(Stream Stream, string ContentType)> GetFileStreamAsync(string storedUrl);

        // Converts a stored key (e.g. "profile-photos/12/abc.jpg") into a full,
        // publicly accessible URL the frontend can use directly in an <img> tag.
        // Returns null if storedKey is null — keeps callers simple (no null-checks
        // scattered everywhere before calling this).
        string? GetPublicUrl(string? storedKey);
    }
}
