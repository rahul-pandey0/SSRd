using SSRd.Models;

namespace SSRd.Services;

public interface IJwtService
{
    (string Token, DateTime ExpiresAt) GenerateToken(TmMembership member);
}
