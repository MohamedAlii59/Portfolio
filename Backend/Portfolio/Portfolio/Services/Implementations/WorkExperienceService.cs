using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.DTOs;
using Portfolio.Models;
using Portfolio.Services.Interfaces;

namespace Portfolio.Services.Implementations
{
    public class WorkExperienceService : IWorkExperienceService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public WorkExperienceService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<WorkExperienceDto>> GetAllForUserAsync(int userId)
        {
            var entries = await _db.WorkExperiences
                .Where(w => w.UserId == userId)
                .OrderBy(w => w.DisplayOrder)
                .ToListAsync();

            return _mapper.Map<List<WorkExperienceDto>>(entries);
        }

        public async Task<WorkExperienceDto> CreateAsync(int userId, UpsertWorkExperienceDto dto)
        {
            var maxOrder = await _db.WorkExperiences
                .Where(w => w.UserId == userId)
                .Select(w => (int?)w.DisplayOrder)
                .MaxAsync() ?? -1;

            var entity = _mapper.Map<WorkExperience>(dto);
            entity.UserId = userId;
            entity.DisplayOrder = maxOrder + 1;

            _db.WorkExperiences.Add(entity);
            await _db.SaveChangesAsync();

            return _mapper.Map<WorkExperienceDto>(entity);
        }

        public async Task<WorkExperienceDto> UpdateAsync(int userId, int experienceId, UpsertWorkExperienceDto dto)
        {
            var entity = await _db.WorkExperiences
                .FirstOrDefaultAsync(w => w.Id == experienceId && w.UserId == userId)
                ?? throw new KeyNotFoundException("Work experience entry not found.");

            _mapper.Map(dto, entity);
            await _db.SaveChangesAsync();

            return _mapper.Map<WorkExperienceDto>(entity);
        }

        public async Task DeleteAsync(int userId, int experienceId)
        {
            var entity = await _db.WorkExperiences
                .FirstOrDefaultAsync(w => w.Id == experienceId && w.UserId == userId)
                ?? throw new KeyNotFoundException("Work experience entry not found.");

            _db.WorkExperiences.Remove(entity);
            await _db.SaveChangesAsync();
        }

        public async Task ReorderAsync(int userId, ReorderRequestDto dto)
        {
            var entries = await _db.WorkExperiences
                .Where(w => w.UserId == userId && dto.OrderedIds.Contains(w.Id))
                .ToListAsync();

            for (int i = 0; i < dto.OrderedIds.Count; i++)
            {
                var entry = entries.FirstOrDefault(w => w.Id == dto.OrderedIds[i]);
                if (entry != null) entry.DisplayOrder = i;
            }

            await _db.SaveChangesAsync();
        }
    }
}
