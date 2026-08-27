using Portfolio.DTOs;
namespace Portfolio.Services.Interfaces
{
    public interface IWorkExperienceService
    {
        Task<List<WorkExperienceDto>> GetAllForUserAsync(int userId);
        Task<WorkExperienceDto> CreateAsync(int userId, UpsertWorkExperienceDto dto);
        Task<WorkExperienceDto> UpdateAsync(int userId, int experienceId, UpsertWorkExperienceDto dto);
        Task DeleteAsync(int userId, int experienceId);
        Task ReorderAsync(int userId, ReorderRequestDto dto);
    }
}
