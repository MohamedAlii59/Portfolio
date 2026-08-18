using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Services.Interfaces;
using Portfolio.DTOs;
namespace Portfolio.Services.Implementations
{
    using AutoMapper;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    public class ProfileService : IProfileService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorage;

        private static readonly string[] AllowedImageTypes = { "image/jpeg", "image/png", "image/webp" };
        private static readonly string[] AllowedResumeTypes = { "application/pdf" };
        private const long MaxImageSize = 5 * 1024 * 1024;   // 5MB
        private const long MaxResumeSize = 10 * 1024 * 1024; // 10MB

        public ProfileService(AppDbContext db, IMapper mapper, IFileStorageService fileStorage)
        {
            _db = db;
            _mapper = mapper;
            _fileStorage = fileStorage;
        }

        public async Task<ProfileResponseDto?> GetPublicProfileBySlugAsync(string slug)
        {
            var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Slug == slug);
            return user == null ? null : MapWithResolvedUrls(user);
        }

        public async Task<ProfileResponseDto?> GetMyProfileAsync(int userId)
        {
            var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == userId);
            return user == null ? null : MapWithResolvedUrls(user);
        }

        public async Task<ProfileResponseDto> UpdateProfileAsync(int userId, UpdateProfileRequestDto dto)
        {
            var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new KeyNotFoundException("User not found.");

            var slugTaken = await _db.Users.IgnoreQueryFilters()
                .AnyAsync(u => u.Slug == dto.Slug && u.Id != userId);
            if (slugTaken)
                throw new InvalidOperationException("That URL is already taken.");

            _mapper.Map(dto, user);
            await _db.SaveChangesAsync();

            return MapWithResolvedUrls(user);
        }

        public async Task<ProfileResponseDto> UploadProfilePhotoAsync(int userId, IFormFile file)
        {
            var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new KeyNotFoundException("User not found.");

            var key = await _fileStorage.SaveFileAsync(file, $"profile-photos/{userId}", AllowedImageTypes, MaxImageSize);

            var oldUrl = user.ProfileImageUrl;
            user.ProfileImageUrl = key;
            await _db.SaveChangesAsync();

            // Delete the old file only after the new one is confirmed saved.
            await _fileStorage.DeleteFileAsync(oldUrl);

            return MapWithResolvedUrls(user);
        }

        public async Task<ProfileResponseDto> UploadOrReplaceResumeAsync(int userId, IFormFile file)
        {
            var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new KeyNotFoundException("User not found.");

            var key = await _fileStorage.SaveFileAsync(file, $"resumes/{userId}", AllowedResumeTypes, MaxResumeSize);

            var oldUrl = user.ResumeUrl;
            user.ResumeUrl = key;
            user.ResumeFileName = file.FileName;
            await _db.SaveChangesAsync();

            await _fileStorage.DeleteFileAsync(oldUrl); // removes the previous resume, if any

            return MapWithResolvedUrls(user);
        }

        public async Task DeleteResumeAsync(int userId)
        {
            var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new KeyNotFoundException("User not found.");

            if (user.ResumeUrl == null) return; // already has no resume — nothing to do

            await _fileStorage.DeleteFileAsync(user.ResumeUrl);
            user.ResumeUrl = null;
            user.ResumeFileName = null;
            await _db.SaveChangesAsync();
        }

        public async Task<(Stream Stream, string ContentType, string FileName)?> DownloadResumeAsync(string slug)
        {
            var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Slug == slug);
            if (user?.ResumeUrl == null) return null;

            var (stream, contentType) = await _fileStorage.GetFileStreamAsync(user.ResumeUrl);
            return (stream, contentType, user.ResumeFileName ?? "resume.pdf");
        }

        // --- Private helper: single place where entity -> DTO + public URL resolution happens ---
        private ProfileResponseDto MapWithResolvedUrls(Models.User user)
        {
            var dto = _mapper.Map<ProfileResponseDto>(user);
            dto.ProfileImageUrl = _fileStorage.GetPublicUrl(user.ProfileImageUrl);
            return dto;
        }
    }
}
