using Microsoft.AspNetCore.Http;
using Portfolio.DTOs;
namespace Portfolio.Services.Interfaces
{
    public interface IProfileService
    {
        // Public: viewed by anyone on the public portfolio site, resolved by slug
        Task<ProfileResponseDto?> GetPublicProfileBySlugAsync(string slug);

        // Admin: the logged-in client viewing/editing their own profile
        Task<ProfileResponseDto?> GetMyProfileAsync(int userId);
        Task<ProfileResponseDto> UpdateProfileAsync(int userId, UpdateProfileRequestDto dto);

        Task<ProfileResponseDto> UploadProfilePhotoAsync(int userId, IFormFile file);

        // Resume: upload acts as "replace" if one already exists
        Task<ProfileResponseDto> UploadOrReplaceResumeAsync(int userId, IFormFile file);
        Task DeleteResumeAsync(int userId);
        Task<(Stream Stream, string ContentType, string FileName)?> DownloadResumeAsync(string slug);
    }
}
