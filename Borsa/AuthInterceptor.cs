using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Borsa.DTO.Authorization;
using Borsa.Extensions;
using Borsa.Services.Abstract;

namespace Borsa
{
    public class AuthInterceptor : DelegatingHandler
    {
        private readonly ILoginService _loginService;
        private readonly ITokenStorage _jsonFileTokenStorage;
        private static readonly SemaphoreSlim SemaphoreSlim = new(1);

        public AuthInterceptor(ILoginService loginService, ITokenStorage jsonFileTokenStorage)
        {
            _loginService = loginService;
            _jsonFileTokenStorage = jsonFileTokenStorage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await SemaphoreSlim.WaitAsync(cancellationToken);

            try
            {
                var token = await _jsonFileTokenStorage.GetToken();

                if (token.IsExpired())
                {
                    token = await _loginService
                        .RefreshTokenAsync(new RefreshTokenQuery(token.RefreshToken));

                    await _jsonFileTokenStorage.SaveToken(token);
                }
                
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

                var response = await base.SendAsync(request, cancellationToken);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    token = await _loginService
                        .RefreshTokenAsync(new RefreshTokenQuery(token.RefreshToken));

                    await _jsonFileTokenStorage.SaveToken(token);

                    request.Headers.Add("Authorization", $"Bearer {token.Token}");
                }

                return await base.SendAsync(request, cancellationToken);
            }
            finally
            {
                SemaphoreSlim.Release();
            }
        }
    }
}