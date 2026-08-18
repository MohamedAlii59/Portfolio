using Portfolio.Models;

namespace Portfolio.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
