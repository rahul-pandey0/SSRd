using SSRd.Models;

namespace SSRd.Services;

public interface IJwtService
{
    (string Token, DateTime ExpiresAt) GenerateToken(TmMembership member);
    (string Token, DateTime ExpiresAt) GenerateAdminToken(TbUserPassword admin);
}
