using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.DTOs;
using Portfolio.Services.Interfaces;


namespace Portfolio.Controllers
{
    
    [ApiController]
    [Route("api/experience")]
    public class ExperienceController : ControllerBase
    {
        private readonly IWorkExperienceService _experienceService;
        private readonly ICurrentUserService _currentUser;

        public ExperienceController(IWorkExperienceService experienceService, ICurrentUserService currentUser)
        {
            _experienceService = experienceService;
            _currentUser = currentUser;
        }

        [HttpGet("{userId:int}")]
        public async Task<IActionResult> GetForUser(int userId)
        {
            return Ok(await _experienceService.GetAllForUserAsync(userId));
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMine()
        {
            return Ok(await _experienceService.GetAllForUserAsync(_currentUser.UserId!.Value));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(UpsertWorkExperienceDto dto)
        {
            var created = await _experienceService.CreateAsync(_currentUser.UserId!.Value, dto);
            return CreatedAtAction(nameof(GetMine), new { }, created);
        }

        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpsertWorkExperienceDto dto)
        {
            try
            {
                return Ok(await _experienceService.UpdateAsync(_currentUser.UserId!.Value, id, dto));
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
                await _experienceService.DeleteAsync(_currentUser.UserId!.Value, id);
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
            await _experienceService.ReorderAsync(_currentUser.UserId!.Value, dto);
            return NoContent();
        }
    }
}
