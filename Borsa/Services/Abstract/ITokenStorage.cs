using System.Threading.Tasks;
using Borsa.DTO.Authorization;

namespace Borsa.Services.Abstract
{
    public interface ITokenStorage
    {
        Task<LogInQueryResult> GetToken();
        
        Task SaveToken(LogInQueryResult logInQueryResult);
    }
}