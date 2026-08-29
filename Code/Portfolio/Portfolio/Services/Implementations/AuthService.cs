using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Services.Interfaces;
using Portfolio.DTOs;
namespace Portfolio.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;

        public AuthService(AppDbContext db, ITokenService tokenService, IEmailService emailService)
        {
            _db = db;
            _tokenService = tokenService;
            _emailService = emailService;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto)
        {
            var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return null;

            return new LoginResponseDto
            {
                Token = _tokenService.GenerateToken(user),
                MustChangePassword = user.MustChangePasswordOnFirstLogin,
                FullName = user.FullName,
                Title = user.Title,
                Slug = user.Slug,
            };
        }

        public async Task RequestPasswordResetAsync(ForgotPasswordRequestDto dto)
        {
            var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return; // silently no-op — controller always returns a generic success message

            user.PasswordResetToken = Guid.NewGuid().ToString("N");
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            await _db.SaveChangesAsync();

            await _emailService.SendPasswordResetEmailAsync(user.Email, user.PasswordResetToken);
        }

        public async Task ResetPasswordAsync(ResetPasswordRequestDto dto)
        {
            var user = await _db.Users.IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.PasswordResetToken == dto.Token);

            if (user == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
                throw new InvalidOperationException("This reset link is invalid or has expired.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            user.MustChangePasswordOnFirstLogin = false;
            await _db.SaveChangesAsync();
        }

        public async Task ChangePasswordAsync(int userId, ChangePasswordRequestDto dto)
        {
            var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new KeyNotFoundException("User not found.");

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                throw new InvalidOperationException("Current password is incorrect.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.MustChangePasswordOnFirstLogin = false;
            await _db.SaveChangesAsync();
        }
    }
}
