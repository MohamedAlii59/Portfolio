using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.DTOs;
using Portfolio.Models;
using Portfolio.Services.Interfaces;

namespace Portfolio.Services.Implementations
{
    public class EducationService : IEducationService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public EducationService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<EducationDto>> GetAllForUserAsync(int userId)
        {
            // Query filter already scopes this to userId, but being explicit here
            // keeps the method correct even if called with IgnoreQueryFilters() elsewhere.
            var entries = await _db.EducationEntries
                .Where(e => e.UserId == userId)
                .OrderBy(e => e.DisplayOrder)
                .ToListAsync();

            return _mapper.Map<List<EducationDto>>(entries);
        }

        public async Task<EducationDto> CreateAsync(int userId, UpsertEducationDto dto)
        {
            var maxOrder = await _db.EducationEntries
                .Where(e => e.UserId == userId)
                .Select(e => (int?)e.DisplayOrder)
                .MaxAsync() ?? -1;

            var entity = _mapper.Map<Education>(dto);
            entity.UserId = userId;
            entity.DisplayOrder = maxOrder + 1;

            _db.EducationEntries.Add(entity);
            await _db.SaveChangesAsync();

            return _mapper.Map<EducationDto>(entity);
        }

        public async Task<EducationDto> UpdateAsync(int userId, int educationId, UpsertEducationDto dto)
        {
            var entity = await _db.EducationEntries
                .FirstOrDefaultAsync(e => e.Id == educationId && e.UserId == userId)
                ?? throw new KeyNotFoundException("Education entry not found.");

            _mapper.Map(dto, entity);
            await _db.SaveChangesAsync();

            return _mapper.Map<EducationDto>(entity);
        }

        public async Task DeleteAsync(int userId, int educationId)
        {
            var entity = await _db.EducationEntries
                .FirstOrDefaultAsync(e => e.Id == educationId && e.UserId == userId)
                ?? throw new KeyNotFoundException("Education entry not found.");

            _db.EducationEntries.Remove(entity);
            await _db.SaveChangesAsync();
        }

        public async Task ReorderAsync(int userId, ReorderRequestDto dto)
        {
            var entries = await _db.EducationEntries
                .Where(e => e.UserId == userId && dto.OrderedIds.Contains(e.Id))
                .ToListAsync();

            for (int i = 0; i < dto.OrderedIds.Count; i++)
            {
                var entry = entries.FirstOrDefault(e => e.Id == dto.OrderedIds[i]);
                if (entry != null) entry.DisplayOrder = i;
            }

            await _db.SaveChangesAsync();
        }
    }
}
