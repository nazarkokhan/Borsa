using System.Threading.Tasks;
using Borsa.DTO.Authorization;

namespace Borsa.Services.Abstract;

public interface ILoginService
{
    public Task<LogInQueryResult> LogInAsync(LogInQuery logInQuery);
    
    public Task<LogInQueryResult> RefreshTokenAsync(RefreshTokenQuery refreshToken);
}