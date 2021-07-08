using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Borsa.Constants;
using Borsa.DTO;
using Borsa.DTO.Alert;
using Borsa.DTO.Alert.Create;
using Borsa.DTO.Enums;
using Borsa.Services.Abstract;

namespace Borsa.Services
{
    public class AlertService : IAlertService
    {
        private readonly HttpClient _httpClient;

        public AlertService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient(Client.AuthClient);
        }

        public async Task<Pager<AlertDto>> GetAlert(int page, int items,
            ActivityStatus? status = null, string search = null)
        {
            var urn = $"Alert/{page}/{items}";

            if (status is not null)
                urn += $"?{nameof(status)}={status.ToString()}";

            if (!string.IsNullOrWhiteSpace(search))
                urn += $"?{nameof(search)}={search}";

            var response =
                await _httpClient.GetAsync(urn);

            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<Pager<AlertDto>>(responseString, new JsonSerializerOptions
            {
                Converters = {new JsonStringEnumConverter()}
            });
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

            var responseString = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<AlertDto>(responseString,
                new JsonSerializerOptions {Converters = {new JsonStringEnumConverter()}}
            );
        }
    }
}