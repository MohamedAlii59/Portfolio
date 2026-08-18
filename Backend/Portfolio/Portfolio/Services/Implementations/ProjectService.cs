using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.DTOs;
using Portfolio.Models;
using Portfolio.Services.Interfaces;

namespace Portfolio.Services.Implementations
{
    public class ProjectService : IProjectService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorage;

        private static readonly string[] AllowedImageTypes = { "image/jpeg", "image/png", "image/webp" };
        private const long MaxImageSize = 5 * 1024 * 1024;

        public ProjectService(AppDbContext db, IMapper mapper, IFileStorageService fileStorage)
        {
            _db = db;
            _mapper = mapper;
            _fileStorage = fileStorage;
        }

        private IQueryable<Project> ProjectsWithDetails(int userId) =>
            _db.Projects
                .Where(p => p.UserId == userId)
                .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
                .Include(p => p.ProjectTechnologies).ThenInclude(pt => pt.Technology);

        public async Task<List<ProjectDto>> GetAllForUserAsync(int userId)
        {
            var projects = await ProjectsWithDetails(userId)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();

            return MapProjects(projects);
        }

        public async Task<ProjectDto?> GetByIdAsync(int userId, int projectId)
        {
            var project = await ProjectsWithDetails(userId).FirstOrDefaultAsync(p => p.Id == projectId);
            return project == null ? null : MapProject(project);
        }

        public async Task<ProjectDto> CreateAsync(int userId, UpsertProjectDto dto)
        {
            var maxOrder = await _db.Projects
                .Where(p => p.UserId == userId)
                .Select(p => (int?)p.DisplayOrder)
                .MaxAsync() ?? -1;

            var entity = _mapper.Map<Project>(dto);
            entity.UserId = userId;
            entity.DisplayOrder = maxOrder + 1;

            await AttachTechnologiesAsync(entity, dto.TechnologyIds);

            _db.Projects.Add(entity);
            await _db.SaveChangesAsync();

            return await GetByIdAsync(userId, entity.Id) ?? throw new InvalidOperationException("Failed to load created project.");
        }

        public async Task<ProjectDto> UpdateAsync(int userId, int projectId, UpsertProjectDto dto)
        {
            var entity = await _db.Projects
                .Include(p => p.ProjectTechnologies)
                .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId)
                ?? throw new KeyNotFoundException("Project not found.");

            _mapper.Map(dto, entity);

            // Replace technology links entirely with the new set sent by the client
            _db.ProjectTechnologies.RemoveRange(entity.ProjectTechnologies);
            entity.ProjectTechnologies.Clear();
            await AttachTechnologiesAsync(entity, dto.TechnologyIds);

            await _db.SaveChangesAsync();

            return await GetByIdAsync(userId, projectId) ?? throw new InvalidOperationException("Failed to load updated project.");
        }

        public async Task DeleteAsync(int userId, int projectId)
        {
            var entity = await _db.Projects
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId)
                ?? throw new KeyNotFoundException("Project not found.");

            foreach (var image in entity.Images)
            {
                await _fileStorage.DeleteFileAsync(image.ImageUrl);
            }

            _db.Projects.Remove(entity); // cascades to ProjectImage / ProjectTechnology rows
            await _db.SaveChangesAsync();
        }

        public async Task ReorderAsync(int userId, ReorderRequestDto dto)
        {
            var projects = await _db.Projects
                .Where(p => p.UserId == userId && dto.OrderedIds.Contains(p.Id))
                .ToListAsync();

            for (int i = 0; i < dto.OrderedIds.Count; i++)
            {
                var project = projects.FirstOrDefault(p => p.Id == dto.OrderedIds[i]);
                if (project != null) project.DisplayOrder = i;
            }

            await _db.SaveChangesAsync();
        }

        public async Task<List<ProjectImageDto>> UploadImagesAsync(int userId, int projectId, List<IFormFile> files)
        {
            var project = await _db.Projects
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId)
                ?? throw new KeyNotFoundException("Project not found.");

            var startOrder = project.Images.Count == 0 ? 0 : project.Images.Max(i => i.DisplayOrder) + 1;
            var uploaded = new List<ProjectImage>();

            foreach (var file in files)
            {
                var key = await _fileStorage.SaveFileAsync(file, $"project-images/{userId}/{projectId}", AllowedImageTypes, MaxImageSize);
                var image = new ProjectImage
                {
                    ProjectId = projectId,
                    ImageUrl = key,
                    DisplayOrder = startOrder + uploaded.Count,
                };
                uploaded.Add(image);
            }

            _db.ProjectImages.AddRange(uploaded);
            await _db.SaveChangesAsync();

            return _mapper.Map<List<ProjectImageDto>>(uploaded);
        }

        public async Task DeleteImageAsync(int userId, int projectId, int imageId)
        {
            var image = await _db.ProjectImages
                .Include(i => i.Project)
                .FirstOrDefaultAsync(i => i.Id == imageId && i.ProjectId == projectId && i.Project!.UserId == userId)
                ?? throw new KeyNotFoundException("Image not found.");

            await _fileStorage.DeleteFileAsync(image.ImageUrl);
            _db.ProjectImages.Remove(image);
            await _db.SaveChangesAsync();
        }

        public async Task ReorderImagesAsync(int userId, int projectId, ReorderRequestDto dto)
        {
            var images = await _db.ProjectImages
                .Include(i => i.Project)
                .Where(i => i.ProjectId == projectId && i.Project!.UserId == userId && dto.OrderedIds.Contains(i.Id))
                .ToListAsync();

            for (int i = 0; i < dto.OrderedIds.Count; i++)
            {
                var image = images.FirstOrDefault(img => img.Id == dto.OrderedIds[i]);
                if (image != null) image.DisplayOrder = i;
            }

            await _db.SaveChangesAsync();
        }

        public async Task<List<ProjectDto>> GetPublicProjectsByUserIdAsync(int userId)
        {
            var projects = await _db.Projects.IgnoreQueryFilters()
                .Where(p => p.UserId == userId)
                .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
                .Include(p => p.ProjectTechnologies).ThenInclude(pt => pt.Technology)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();

            return MapProjects(projects);
        }

        public async Task<ProjectDto?> GetPublicProjectByIdAsync(int userId, int projectId)
        {
            var project = await _db.Projects.IgnoreQueryFilters()
                .Where(p => p.UserId == userId && p.Id == projectId)
                .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
                .Include(p => p.ProjectTechnologies).ThenInclude(pt => pt.Technology)
                .FirstOrDefaultAsync();

            return project == null ? null : MapProject(project);
        }

        // --- Private helpers ---

        private async Task AttachTechnologiesAsync(Project project, List<int> technologyIds)
        {
            if (technologyIds.Count == 0) return;

            var validIds = await _db.Technologies
                .Where(t => technologyIds.Contains(t.Id))
                .Select(t => t.Id)
                .ToListAsync();

            foreach (var techId in validIds)
            {
                project.ProjectTechnologies.Add(new ProjectTechnology { TechnologyId = techId });
            }
        }

        private ProjectDto MapProject(Project project)
        {
            var dto = _mapper.Map<ProjectDto>(project);
            dto.Technologies = _mapper.Map<List<TechnologyDto>>(project.ProjectTechnologies.Select(pt => pt.Technology));

            foreach (var image in dto.Images)
            {
                var matchingEntity = project.Images.First(i => i.Id == image.Id);
                image.ImageUrl = _fileStorage.GetPublicUrl(matchingEntity.ImageUrl)!;
            }

            return dto;
        }

        private List<ProjectDto> MapProjects(List<Project> projects) => projects.Select(MapProject).ToList();
    }
}
