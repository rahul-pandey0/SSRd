using System.ComponentModel.DataAnnotations;

namespace SSRd.DTOs;

public class InitiateRequest
{
    [Required]
    [Phone]
    public string PhoneNo { get; set; } = string.Empty;
}

public class InitiateResponse
{
    public bool IsExistingMember { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class RegisterRequest
{
    [Required]
    [Phone]
    public string PhoneNo { get; set; } = string.Empty;

    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string Otp { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? FatherName { get; set; }

    [MaxLength(150)]
    public string? Address { get; set; }

    public double Age { get; set; }

    public DateOnly? Birthdate { get; set; }

    [MaxLength(45)]
    public string? Pancard { get; set; }

    [MaxLength(45)]
    public string? AdharCard { get; set; }
}

public class VerifyOtpRequest
{
    [Required]
    [Phone]
    public string PhoneNo { get; set; } = string.Empty;

    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string Otp { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public int MemberId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MembershipNo { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class RegisterResponse
{
    public int MemberId { get; set; }
    public string MembershipNo { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Success") =>
        new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message) =>
        new() { Success = false, Message = message };
}
