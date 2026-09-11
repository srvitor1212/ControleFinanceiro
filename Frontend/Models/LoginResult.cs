namespace Frontend.Models;

public sealed class LoginResult
{
    private const string GenericFailureMessage = "Não foi possível concluir o login. Tente novamente.";

    private LoginResult(bool isSuccess, LoginResponse? response, IReadOnlyList<string> errors)
    {
        IsSuccess = isSuccess;
        Response = response;
        Errors = errors;
    }

    public bool IsSuccess { get; }

    public LoginResponse? Response { get; }

    public IReadOnlyList<string> Errors { get; }

    public string? AccessToken => Response?.AccessToken;

    public string? ExpiresAt => Response?.ExpiresAt;

    public static LoginResult Success(LoginResponse response) => new(true, response, Array.Empty<string>());

    public static LoginResult Rejected(IEnumerable<string> errors)
    {
        var usableErrors = errors
            .Where(error => !string.IsNullOrWhiteSpace(error))
            .Select(error => error.Trim())
            .ToArray();

        return new LoginResult(false, null, usableErrors.Length > 0 ? usableErrors : [GenericFailureMessage]);
    }

    public static LoginResult Failure() => new(false, null, [GenericFailureMessage]);
}
