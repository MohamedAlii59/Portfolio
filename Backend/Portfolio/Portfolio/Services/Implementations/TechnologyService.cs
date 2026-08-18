using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.DTOs;
using Portfolio.Models;
using Portfolio.Services.Interfaces;

namespace Portfolio.Services.Implementations
{
    public class TechnologyService : ITechnologyService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorage;

        private static readonly string[] AllowedIconTypes = { "image/png", "image/svg+xml", "image/webp" };
        private const long MaxIconSize = 1 * 1024 * 1024; // 1MB — icons should be small

        public TechnologyService(AppDbContext db, IMapper mapper, IFileStorageService fileStorage)
        {
            _db = db;
            _mapper = mapper;
            _fileStorage = fileStorage;
        }

        public async Task<List<TechnologyDto>> GetAllAsync()
        {
            var technologies = await _db.Technologies.OrderBy(t => t.Name).ToListAsync();
            return _mapper.Map<List<TechnologyDto>>(technologies);
        }

        public async Task<TechnologyDto> CreateAsync(CreateTechnologyDto dto, IFormFile? icon)
        {
            var entity = new Technology { Name = dto.Name };

            if (icon != null)
            {
                entity.IconUrl = await _fileStorage.SaveFileAsync(icon, "technology-icons", AllowedIconTypes, MaxIconSize);
            }

            _db.Technologies.Add(entity);
            await _db.SaveChangesAsync();

            return _mapper.Map<TechnologyDto>(entity);
        }

        public async Task DeleteAsync(int technologyId)
        {
            var entity = await _db.Technologies
                .Include(t => t.UserTechnologies)
                .Include(t => t.ProjectTechnologies)
                .FirstOrDefaultAsync(t => t.Id == technologyId)
                ?? throw new KeyNotFoundException("Technology not found.");

            // Block deletion if in use — safer than silently unlinking everywhere.
            if (entity.UserTechnologies.Any() || entity.ProjectTechnologies.Any())
                throw new InvalidOperationException(
                    "This technology is currently in use on your profile or projects. Remove it from those first.");

            await _fileStorage.DeleteFileAsync(entity.IconUrl);
            _db.Technologies.Remove(entity);
            await _db.SaveChangesAsync();
        }

        public async Task AddToProfileAsync(int userId, int technologyId)
        {
            var alreadyLinked = await _db.UserTechnologies
                .IgnoreQueryFilters()
                .AnyAsync(ut => ut.UserId == userId && ut.TechnologyId == technologyId);

            if (alreadyLinked) return; // idempotent — adding twice is a no-op, not an error

            var technologyExists = await _db.Technologies.AnyAsync(t => t.Id == technologyId);
            if (!technologyExists) throw new KeyNotFoundException("Technology not found.");

            _db.UserTechnologies.Add(new UserTechnology { UserId = userId, TechnologyId = technologyId });
            await _db.SaveChangesAsync();
        }

        public async Task RemoveFromProfileAsync(int userId, int technologyId)
        {
            var link = await _db.UserTechnologies.IgnoreQueryFilters()
                .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TechnologyId == technologyId);

            if (link == null) return; // already not linked — nothing to do

            _db.UserTechnologies.Remove(link);
            await _db.SaveChangesAsync();
        }

        public async Task<List<TechnologyDto>> GetProfileTechnologiesAsync(int userId)
        {
            var technologies = await _db.UserTechnologies
                .IgnoreQueryFilters()
                .Where(ut => ut.UserId == userId)
                .Select(ut => ut.Technology!)
                .ToListAsync();

            return _mapper.Map<List<TechnologyDto>>(technologies);
        }
    }
}
