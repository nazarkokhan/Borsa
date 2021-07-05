using System.Threading.Tasks;
using Borsa.DTO;

namespace Borsa.Services.Abstract
{
    public interface ILoginService
    {
        public Task<TokenDto> LogInAsync(LogInDto logInDto);
        public Task<TokenDto> RefreshTokenAsync(RefreshTokenDto refreshToken);
    }
}