using System.Threading.Tasks;
using Borsa.DTO.Alert;
using Borsa.DTO.Alert.Create;

namespace Borsa.Services.Abstract
{
    public interface IAlertService
    {
        Task<AlertDto> CreateAlert(CreateAlertDto alertDto);
    }
}