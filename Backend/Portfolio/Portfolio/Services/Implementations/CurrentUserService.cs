using Portfolio.Services.Interfaces;

namespace Portfolio.Services.Implementations
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? UserId
        {
            get
            {
                var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId");
                if (claim == null) return null;
                return int.TryParse(claim.Value, out var id) ? id : null;
            }
        }
    }
}
