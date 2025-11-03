namespace user_service.DTOs;

public class RegisterResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public Guid? UserId { get; set; }
    public string? VerificationToken { get; set; }

    public static RegisterResult SuccessResult(Guid userId, string verificationToken)
    {
        return new RegisterResult
        {
            Success = true,
            UserId = userId,
            VerificationToken = verificationToken
        };
    }

    public static RegisterResult FailResult(string errorMessage, string errorCode)
    {
        return new RegisterResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode
        };
    }
}

public class VerifyEmailResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }

    public static VerifyEmailResult SuccessResult()
    {
        return new VerifyEmailResult { Success = true };
    }

    public static VerifyEmailResult FailResult(string errorMessage, string errorCode)
    {
        return new VerifyEmailResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode
        };
    }
}


