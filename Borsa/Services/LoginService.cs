using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Borsa.DTO;
using Borsa.Services.Abstract;

namespace Borsa.Services
{
    public class LoginService : ILoginService
    {
        private readonly HttpClient _httpClient;

        public LoginService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient(nameof(LoginService));
        }

        public async Task<TokenDto> LogInAsync(LogInDto logInDto)
        {
            var json = JsonSerializer.Serialize(logInDto);

            var response = await _httpClient
                .PostAsync(
                    "Auth/token",
                    new StringContent(json, Encoding.UTF8, "application/json")
                );

            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<TokenDto>(responseBody);
        }
        
        public async Task<TokenDto> RefreshTokenAsync(RefreshTokenDto refreshToken)
        {
            var json = JsonSerializer.Serialize(refreshToken);

            var response = await _httpClient
                .PostAsync(
                    "Auth/refresh-token",
                    new StringContent(json, Encoding.UTF8, "application/json")
                );

            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<TokenDto>(responseBody);
        }
    }
}