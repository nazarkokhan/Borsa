using System.Threading.Tasks;
using Borsa.DTO;
using Borsa.DTO.Alert;

namespace Borsa.Services.Abstract;

public interface IInstrumentService
{
    Task<Pager<InstrumentDto>> GetInstrument(int page, int items, 
        string search = null, string orderType = null, string country = null);
}