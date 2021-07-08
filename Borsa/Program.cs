using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Borsa.Constants;
using Borsa.DTO;
using Borsa.DTO.Alert;
using Borsa.DTO.Alert.Create;
using Borsa.Services;
using Borsa.Services.Abstract;
using Microsoft.Extensions.DependencyInjection;
using Borsa.DTO.Authorization;
using Borsa.DTO.Enums;
using Microsoft.AspNetCore.SignalR.Client;

// ReSharper disable All

namespace Borsa
{
    [SuppressMessage("ReSharper", "ArgumentsStyleNamedExpression")]
    static class Program
    {
        [SuppressMessage("ReSharper", "ArgumentsStyleStringLiteral")]
        static async Task Main()
        {
            // var con = new HubConnectionBuilder()
            //     .WithUrl()
            //     .Build();
            // con.On("asd", (Tick tick) =>
            // {
            //
            // });
            // con.SendAsync("asd");
            // con.StartAsync()
            var services = new ServiceCollection();

            services
                .AddScoped<ILoginService, LoginService>()
                .AddScoped<IAlertService, AlertService>()
                .AddScoped<IInstrumentService, InstrumentService>()
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
            
            var instrumentService = provider.GetRequiredService<IInstrumentService>();

            var alert1 = new CreateAlertDto
            (
                conditions: new List<CreateConditionDto>
                {
                    new CreateConditionDto
                    (
                        compareType: CompareType.BiggerThen,
                        leftExpression: new CreateExpressionDto
                        (
                            indicatorType: IndicatorType.Price,
                            parameters: new Dictionary<string, string>{ }
                        ),
                        rightExpression: new CreateExpressionDto
                        (
                            IndicatorType.Number,
                            new Dictionary<string, string>
                            {
                                {IndicatorType.Number.ToString(), 100.ToString()},
                            }
                        ),
                        instrumentId: 3842
                    )
                },
                name: "Alert",
                notes: "My Notes",
                buySell: BuySell.Buy,
                autoStart: true
            );

            var someAlerts = await alertService.GetAlert(1, 20);
            
            var someCreatedAlert = await alertService.CreateAlert(alert1);

            var someInstruments = await instrumentService.GetInstrument(1, 20);
            
            Console.WriteLine(someAlerts);

            Console.ReadLine();
        }
    }
}