using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.DTOs;
using Portfolio.Services.Interfaces;

namespace Portfolio.Controllers
{
   
    [ApiController]
    [Route("api/education")]
    public class EducationController : ControllerBase
    {
        private readonly IEducationService _educationService;
        private readonly ICurrentUserService _currentUser;

        public EducationController(IEducationService educationService, ICurrentUserService currentUser)
        {
            _educationService = educationService;
            _currentUser = currentUser;
        }

        // Public: powers the public portfolio's education section
        [HttpGet("{userId:int}")]
        public async Task<IActionResult> GetForUser(int userId)
        {
            return Ok(await _educationService.GetAllForUserAsync(userId));
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMine()
        {
            return Ok(await _educationService.GetAllForUserAsync(_currentUser.UserId!.Value));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(UpsertEducationDto dto)
        {
            var created = await _educationService.CreateAsync(_currentUser.UserId!.Value, dto);
            return CreatedAtAction(nameof(GetMine), new { }, created);
        }

        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpsertEducationDto dto)
        {
            try
            {
                return Ok(await _educationService.UpdateAsync(_currentUser.UserId!.Value, id, dto));
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
                await _educationService.DeleteAsync(_currentUser.UserId!.Value, id);
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
            await _educationService.ReorderAsync(_currentUser.UserId!.Value, dto);
            return NoContent();
        }
    }
}
