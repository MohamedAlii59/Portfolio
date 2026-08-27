using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.DTOs;
using Portfolio.Services.Interfaces;

namespace Portfolio.Controllers
{
    [ApiController]
    [Route("api/technologies")]
    public class TechnologiesController : ControllerBase
    {
        private readonly ITechnologyService _technologyService;
        private readonly ICurrentUserService _currentUser;

        public TechnologiesController(ITechnologyService technologyService, ICurrentUserService currentUser)
        {
            _technologyService = technologyService;
            _currentUser = currentUser;
        }

        // Public: full list, used by the admin tech picker and public profile display
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _technologyService.GetAllAsync());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateTechnologyRequestDto request)
        {
            var created = await _technologyService.CreateAsync(
                new CreateTechnologyDto { Name = request.Name },
                request.Icon
            );
            return CreatedAtAction(nameof(GetAll), new { }, created);
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _technologyService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                // Deleting a technology still in use is blocked, not silently cascaded
                return Conflict(new { message = ex.Message });
            }
        }

        // --- Profile-level technology links ("my skills") ---

        [HttpGet("profile/{userId:int}")]
        public async Task<IActionResult> GetProfileTechnologies(int userId)
        {
            return Ok(await _technologyService.GetProfileTechnologiesAsync(userId));
        }

        [Authorize]
        [HttpPost("profile/{technologyId:int}")]
        public async Task<IActionResult> AddToProfile(int technologyId)
        {
            try
            {
                await _technologyService.AddToProfileAsync(_currentUser.UserId!.Value, technologyId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [Authorize]
        [HttpDelete("profile/{technologyId:int}")]
        public async Task<IActionResult> RemoveFromProfile(int technologyId)
        {
            await _technologyService.RemoveFromProfileAsync(_currentUser.UserId!.Value, technologyId);
            return NoContent();
        }
    }
}
