using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Borsa.Constants;
using Borsa.DTO.Alert;
using Borsa.DTO.Alert.Create;
using Borsa.Services.Abstract;

namespace Borsa.Services
{
    public class AlertService : IAlertService
    {
        private readonly HttpClient _httpClient;

        private readonly ITokenStorage _jsonFileTokenStorage;

        public AlertService(IHttpClientFactory httpClientFactory, ITokenStorage jsonFileTokenStorage)
        {
            _jsonFileTokenStorage = jsonFileTokenStorage;
            _httpClient = httpClientFactory.CreateClient(Client.AuthClient);
        }

        public async Task<AlertDto> CreateAlert(CreateAlertDto alertDto)
        {
            var json = JsonSerializer.Serialize(alertDto);

            var response = await _httpClient
                .PostAsync(
                    "Alert",
                    new StringContent(json, Encoding.UTF8, "application/json")
                );

            response.EnsureSuccessStatusCode();
            
            return JsonSerializer.Deserialize<AlertDto>(await response.Content.ReadAsStringAsync());
        }
    }
}