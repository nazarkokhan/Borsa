using System.Threading.Tasks;
using Borsa.DTO;
using Borsa.DTO.Alert;
using Borsa.DTO.Alert.Create;
using Borsa.DTO.Enums;

namespace Borsa.Services.Abstract
{
    public interface IAlertService
    {
        Task<Pager<AlertDto>> GetAlert(int page, int items, ActivityStatus? status = null, string search = null);
        
        Task<AlertDto> CreateAlert(CreateAlertDto alertDto);
    }
}