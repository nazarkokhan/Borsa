using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Borsa.Constants;
using Borsa.DTO.Authorization;
using Borsa.Services.Abstract;

namespace Borsa.Services
{
    using System.Net;

    public class LoginService : ILoginService
    {
        private readonly HttpClient _httpClient;

        private readonly ITokenStorage _jsonFileTokenStorage;

        public LoginService(IHttpClientFactory httpClientFactory, ITokenStorage tokenStorage)
        {
            _httpClient = httpClientFactory.CreateClient(nameof(LoginService));

            _jsonFileTokenStorage = tokenStorage;
        }

        public async Task<LogInQueryResult> LogInAsync(LogInQuery logInQuery)
        {
            var json = JsonSerializer.Serialize(logInQuery);

            var response = await _httpClient
                .PostAsync(
                    "Auth/LogIn",
                    new StringContent(json, Encoding.UTF8, FileName.Json)
                );

            response.EnsureSuccessStatusCode();

            var responseToken = JsonSerializer
                .Deserialize<LogInQueryResult>(
                    await response.Content.ReadAsStringAsync()
                );

            await _jsonFileTokenStorage.SaveToken(responseToken);

            return responseToken;
        }

        public async Task<LogInQueryResult> RefreshTokenAsync(RefreshTokenQuery refreshToken)
        {
            var json = JsonSerializer.Serialize(refreshToken);

            var response = await _httpClient
                .PostAsync(
                    "Auth/RefreshToken",
                    new StringContent(json, Encoding.UTF8, FileName.Json));

            if (response.StatusCode == HttpStatusCode.BadRequest)
                return null!;
            
            response.EnsureSuccessStatusCode();

            var responseToken = JsonSerializer
                .Deserialize<LogInQueryResult>(
                    await response.Content.ReadAsStringAsync());

            await _jsonFileTokenStorage.SaveToken(responseToken);

            return responseToken;
        }
    }
}