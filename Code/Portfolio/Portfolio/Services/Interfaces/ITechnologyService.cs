using Microsoft.AspNetCore.Http;
using Portfolio.DTOs;

namespace Portfolio.Services.Interfaces
{
    public interface ITechnologyService
    {
        Task<List<TechnologyDto>> GetAllAsync();
        Task<TechnologyDto> CreateAsync(CreateTechnologyDto dto, IFormFile? icon);
        Task DeleteAsync(int technologyId);

        Task AddToProfileAsync(int userId, int technologyId);
        Task RemoveFromProfileAsync(int userId, int technologyId);
        Task<List<TechnologyDto>> GetProfileTechnologiesAsync(int userId);
    }
}
