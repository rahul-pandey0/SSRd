namespace SSRd.Services;

public interface ILoginAuditService
{
    Task<int> RecordLoginAsync(int? userId, string? userName, string? ipAddress, string? branchCode);
    Task RecordLogoutAsync(int loginId);
}
