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
                // Получаю токен з стореджу
                var token = await _jsonFileTokenStorage.GetToken();

                // Якщо токен експайред
                if (token.IsExpired())
                {
                    // Получаю новий ассес і рефреш токен по рефреш токену
                    token = await _loginService
                        .RefreshTokenAsync(new RefreshTokenQuery(token.RefreshToken));

                    // Якщо на рефреш токен приходить бед реквест (рефреш токен не вірний) тоді повертаємо 403
                    // (бо 2 токена вже не валідні) і можна викидати юреза на сторінку логіну 
                    if (token is null)
                        return new HttpResponseMessage(HttpStatusCode.Unauthorized);

                    // Якщо токен рефрешнувся тоді перезаписуємо його в сторедж
                    await _jsonFileTokenStorage.SaveToken(token);
                }

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

                // Шлемо реквест нарешті
                var response = await base.SendAsync(request, cancellationToken);

                //Якшо на реквест приходить 403 рефрешимо токен(Все так само як зверху)
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    token = await _loginService
                        .RefreshTokenAsync(new RefreshTokenQuery(token.RefreshToken));

                    if (token is null)
                        return new HttpResponseMessage(HttpStatusCode.Unauthorized);

                    await _jsonFileTokenStorage.SaveToken(token);

                    request.Headers.Add("Authorization", $"Bearer {token.Token}");
                }
                else
                {
                    // Якшо то не 403 а щось інше просто повертаємо відповідь від сервера
                    return response;
                }

                // Цей код виконується якщо нам на 1 реквест прийшлов 403 код
                // В такому випадку шлемо реквест ще раз але вже з оновленим токеном
                return await base.SendAsync(request, cancellationToken);
            }
            finally
            {
                SemaphoreSlim.Release();
            }
        }
    }
}