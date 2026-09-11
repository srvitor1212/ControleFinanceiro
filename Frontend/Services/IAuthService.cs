using Frontend.Models;

namespace Frontend.Services;

public interface IAuthService
{
    Task<LoginResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
}
