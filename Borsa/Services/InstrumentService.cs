using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Borsa.Constants;
using Borsa.DTO;
using Borsa.DTO.Alert;
using Borsa.DTO.Alert.Create;
using Borsa.Services.Abstract;

namespace Borsa.Services
{
    public class InstrumentService : IInstrumentService
    {
        private readonly HttpClient _httpClient;

        public InstrumentService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient(Client.AuthClient);
        }

        public async Task<Pager<InstrumentDto>> GetInstrument(int page, int items, 
            string search = null, string orderType = null, string country = null)
        {
            var urn = $"Instrument/{page}/{items}";

            if (!string.IsNullOrWhiteSpace(search))
                urn += $"?{nameof(search)}={search}";
            
            if (!string.IsNullOrWhiteSpace(orderType))
                urn += $"?{nameof(orderType)}={orderType}";
            
            if (!string.IsNullOrWhiteSpace(country))
                urn += $"?{nameof(country)}={country}";
            
            var response = 
                await _httpClient.GetAsync(urn);

            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<Pager<InstrumentDto>>(await response.Content.ReadAsStringAsync());
        }
    }
}