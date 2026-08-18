using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.DTOs;
using Portfolio.Services.Interfaces;

namespace Portfolio.Controllers
{
    [ApiController]
    [Route("api/projects")]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly ICurrentUserService _currentUser;

        public ProjectsController(IProjectService projectService, ICurrentUserService currentUser)
        {
            _projectService = projectService;
            _currentUser = currentUser;
        }

        // --- Public: powers the public portfolio site ---

        [HttpGet("public/{userId:int}")]
        public async Task<IActionResult> GetPublicProjects(int userId)
        {
            return Ok(await _projectService.GetPublicProjectsByUserIdAsync(userId));
        }

        [HttpGet("public/{userId:int}/{projectId:int}")]
        public async Task<IActionResult> GetPublicProject(int userId, int projectId)
        {
            var project = await _projectService.GetPublicProjectByIdAsync(userId, projectId);
            return project == null ? NotFound() : Ok(project);
        }

        // --- Admin: the logged-in client managing their own projects ---

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMine()
        {
            return Ok(await _projectService.GetAllForUserAsync(_currentUser.UserId!.Value));
        }

        [Authorize]
        [HttpGet("me/{id:int}")]
        public async Task<IActionResult> GetMineById(int id)
        {
            var project = await _projectService.GetByIdAsync(_currentUser.UserId!.Value, id);
            return project == null ? NotFound() : Ok(project);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(UpsertProjectDto dto)
        {
            var created = await _projectService.CreateAsync(_currentUser.UserId!.Value, dto);
            return CreatedAtAction(nameof(GetMineById), new { id = created.Id }, created);
        }

        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpsertProjectDto dto)
        {
            try
            {
                return Ok(await _projectService.UpdateAsync(_currentUser.UserId!.Value, id, dto));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _projectService.DeleteAsync(_currentUser.UserId!.Value, id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [Authorize]
        [HttpPost("reorder")]
        public async Task<IActionResult> Reorder(ReorderRequestDto dto)
        {
            await _projectService.ReorderAsync(_currentUser.UserId!.Value, dto);
            return NoContent();
        }

        // --- Project images ---

        [Authorize]
        [HttpPost("{id:int}/images")]
        public async Task<IActionResult> UploadImages(int id, [FromForm] List<IFormFile> files)
        {
            try
            {
                var images = await _projectService.UploadImagesAsync(_currentUser.UserId!.Value, id, files);
                return Ok(images);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("{id:int}/images/{imageId:int}")]
        public async Task<IActionResult> DeleteImage(int id, int imageId)
        {
            try
            {
                await _projectService.DeleteImageAsync(_currentUser.UserId!.Value, id, imageId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [Authorize]
        [HttpPost("{id:int}/images/reorder")]
        public async Task<IActionResult> ReorderImages(int id, ReorderRequestDto dto)
        {
            await _projectService.ReorderImagesAsync(_currentUser.UserId!.Value, id, dto);
            return NoContent();
        }
    }
}
