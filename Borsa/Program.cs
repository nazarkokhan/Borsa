using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Borsa.Constants;
using Borsa.DTO;
using Borsa.Services;
using Borsa.Services.Abstract;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Borsa
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();

            services.AddHttpClient(nameof(LoginService), client =>
            {
                client.BaseAddress = new Uri("https://borsa.levent.io");
            }).Services.AddScoped<ILoginService, LoginService>();
            
            LogInDto logInDto =
                new LogInDto(LoginData.Email, LoginData.Password, new DeviceDto("RandomToken", "Android"));
            
            var provider = services.BuildServiceProvider();

            var loginService = provider.GetRequiredService<ILoginService>();

            var token = await loginService!.LogInAsync(logInDto);

            var refreshToken = await loginService!.RefreshTokenAsync(new RefreshTokenDto(token.RefreshToken));
            
            Console.WriteLine(refreshToken.Token);
            
            Console.ReadLine();
        }
    }
}