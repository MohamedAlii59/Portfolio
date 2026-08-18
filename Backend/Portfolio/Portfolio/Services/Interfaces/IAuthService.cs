using Portfolio.DTOs;
namespace Portfolio.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto);
        Task RequestPasswordResetAsync(ForgotPasswordRequestDto dto);
        Task ResetPasswordAsync(ResetPasswordRequestDto dto);
        Task ChangePasswordAsync(int userId, ChangePasswordRequestDto dto);
    }
}
