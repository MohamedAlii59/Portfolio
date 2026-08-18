using Portfolio.DTOs;
namespace Portfolio.Services.Interfaces
{
    public interface IEducationService
    {
        Task<List<EducationDto>> GetAllForUserAsync(int userId);
        Task<EducationDto> CreateAsync(int userId, UpsertEducationDto dto);
        Task<EducationDto> UpdateAsync(int userId, int educationId, UpsertEducationDto dto);
        Task DeleteAsync(int userId, int educationId);
        Task ReorderAsync(int userId, ReorderRequestDto dto);
    }
}
