using Microsoft.AspNetCore.Http;
using Portfolio.DTOs;

namespace Portfolio.Services.Interfaces
{
    public interface IProjectService
    {
        Task<List<ProjectDto>> GetAllForUserAsync(int userId);
        Task<ProjectDto?> GetByIdAsync(int userId, int projectId);
        Task<ProjectDto> CreateAsync(int userId, UpsertProjectDto dto);
        Task<ProjectDto> UpdateAsync(int userId, int projectId, UpsertProjectDto dto);
        Task DeleteAsync(int userId, int projectId);
        Task ReorderAsync(int userId, ReorderRequestDto dto);

        Task<List<ProjectImageDto>> UploadImagesAsync(int userId, int projectId, List<IFormFile> files);
        Task DeleteImageAsync(int userId, int projectId, int imageId);
        Task ReorderImagesAsync(int userId, int projectId, ReorderRequestDto dto);

        // Public: used by the public portfolio site (no auth, resolved by slug elsewhere)
        Task<List<ProjectDto>> GetPublicProjectsByUserIdAsync(int userId);
        Task<ProjectDto?> GetPublicProjectByIdAsync(int userId, int projectId);
    }
}
