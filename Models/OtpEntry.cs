namespace SSRd.Models;

public record OtpEntry(string Code, DateTime ExpiresAt, bool IsVerified = false);
