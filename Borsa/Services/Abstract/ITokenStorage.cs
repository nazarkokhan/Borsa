using System.Threading.Tasks;
using Borsa.DTO.Authorization;

namespace Borsa.Services.Abstract
{
    public interface ITokenStorage
    {
        Task<TokenDto> GetToken();
        
        Task SaveToken(TokenDto tokenDto);
    }
}