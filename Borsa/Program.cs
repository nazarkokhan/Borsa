using System;
using System.Threading.Tasks;
using Borsa.Constants;
using Borsa.DTO;
using Borsa.Services;
using Borsa.Services.Abstract;
using Microsoft.Extensions.DependencyInjection;
using Borsa.DTO.Authorization;

namespace Borsa
{
    static class Program
    {
        static async Task Main()
        {
            var services = new ServiceCollection();

            services
                .AddScoped<ILoginService, LoginService>()
                .AddScoped<IAlertService, AlertService>()
                .AddSingleton<ITokenStorage, JsonFileTokenStorage>()
                .AddScoped<AuthInterceptor>()
                .AddHttpClient(
                    nameof(LoginService),
                    client => { client.BaseAddress = new Uri("https://borsa.levent.io"); }
                )
                .Services
                .AddHttpClient(
                    "AuthClient",
                    client => { client.BaseAddress = new Uri("https://borsa.levent.io"); }
                )
                .AddHttpMessageHandler<AuthInterceptor>();

            var provider = services.BuildServiceProvider();

            var logInService = provider.GetRequiredService<ILoginService>();

            await logInService.LogInAsync(new LogInDto(
                LoginData.Email,
                LoginData.Password,
                new DeviceDto("RandomDeviceToken", "Android")
            ));

            var alertService = provider.GetRequiredService<IAlertService>();

            // var alert = new CreateAlertDto(
            //     new List<CreateConditionDto>
            //     {
            //         new CreateConditionDto(CompareType.BiggerThen, default, default ,1)
            //         CompareType.BiggerThen,
            //         new CreateExpressionDto
            //         (
            //             IndicatorType.Number,
            //             default
            //         ),
            //         new CreateExpressionDto
            //         (
            //             IndicatorType.Number,
            //             default
            //         ),
            //         1
            //     },
            //     "Alert",
            //     BuySell.Buy,
            //     true
            // );
            
            var some = await alertService.CreateAlert(default);

            Console.WriteLine(some);

            Console.ReadLine();
        }
    }
}