using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Borsa.DTO.Authorization;
using Borsa.Services.Abstract;

namespace Borsa.Services
{
    public class LoginService : ILoginService
    {
        private readonly HttpClient _httpClient;

        private readonly ITokenStorage _jsonFileTokenStorage;

        public LoginService(IHttpClientFactory httpClientFactory, ITokenStorage tokenStorage)
        {
            _httpClient = httpClientFactory.CreateClient(nameof(LoginService));

            _jsonFileTokenStorage = tokenStorage;
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

            var responseToken = JsonSerializer
                .Deserialize<TokenDto>(
                    await response.Content.ReadAsStringAsync()
                );

            await _jsonFileTokenStorage.SaveToken(responseToken);

            return responseToken;
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

            var responseToken = JsonSerializer
                .Deserialize<TokenDto>(
                    await response.Content.ReadAsStringAsync()
                );

            await _jsonFileTokenStorage.SaveToken(responseToken);

            return responseToken;
        }
    }
}