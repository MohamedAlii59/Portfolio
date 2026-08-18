using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.DTOs;
using Portfolio.Services.Interfaces;

namespace Portfolio.Controllers
{
   
    [ApiController]
    [Route("api/profile")]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly ICurrentUserService _currentUser;

        public ProfileController(IProfileService profileService, ICurrentUserService currentUser)
        {
            _profileService = profileService;
            _currentUser = currentUser;
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> GetPublicProfile(string slug)
        {
            var profile = await _profileService.GetPublicProfileBySlugAsync(slug);
            return profile == null ? NotFound() : Ok(profile);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var profile = await _profileService.GetMyProfileAsync(_currentUser.UserId!.Value);
            return profile == null ? NotFound() : Ok(profile);
        }

        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileRequestDto dto)
        {
            try
            {
                var updated = await _profileService.UpdateProfileAsync(_currentUser.UserId!.Value, dto);
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("me/photo")]
        public async Task<IActionResult> UploadPhoto([FromForm] IFormFile file)
        {
            try
            {
                var updated = await _profileService.UploadProfilePhotoAsync(_currentUser.UserId!.Value, file);
                return Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("me/resume")]
        public async Task<IActionResult> UploadOrReplaceResume([FromForm] IFormFile file)
        {
            try
            {
                var updated = await _profileService.UploadOrReplaceResumeAsync(_currentUser.UserId!.Value, file);
                return Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("me/resume")]
        public async Task<IActionResult> DeleteResume()
        {
            await _profileService.DeleteResumeAsync(_currentUser.UserId!.Value);
            return NoContent();
        }

        [HttpGet("{slug}/resume/download")]
        public async Task<IActionResult> DownloadResume(string slug)
        {
            var result = await _profileService.DownloadResumeAsync(slug);
            if (result == null) return NotFound();

            var (stream, contentType, fileName) = result.Value;
            // Passing a filename to File() sets Content-Disposition: attachment,
            // which forces a real download instead of an inline preview.
            return File(stream, contentType, fileName);
        }
    }
}
