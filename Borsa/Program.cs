using System;
using System.IO;
using System.Threading.Tasks;
using Borsa.Constants;
using Borsa.Constants.Url;
using Borsa.Services;
using Borsa.Services.Abstract;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Borsa.Extensions;
using Microsoft.AspNetCore.SignalR.Client;

// ReSharper disable All

namespace Borsa
{
    class Program
    {
        public static HubConnection HubConnection { get; set; }
        static async Task Main()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName.AppSettings))
                .Build();

            var provider = new ServiceCollection()
                .AddScoped<ILoginService, LoginService>()
                .AddScoped<IAlertService, AlertService>()
                .AddScoped<IInstrumentService, InstrumentService>()
                .AddSingleton<ITokenStorage, JsonFileTokenStorage>()
                .AddScoped<AuthInterceptor>()
                .AddSingleton<IConfiguration>(configuration)
                .AddHttpClient(
                    nameof(LoginService),
                    client => { client.BaseAddress = new Uri(configuration.GetApiBaseUrl()); }
                )
                .Services
                .AddHttpClient(
                    Client.AuthClient,
                    client => { client.BaseAddress = new Uri(configuration.GetApiBaseUrl()); }
                )
                .AddHttpMessageHandler<AuthInterceptor>()
                .Services
                .BuildServiceProvider();


            var logInService = provider.GetRequiredService<ILoginService>();
            
            var alertService = provider.GetRequiredService<IAlertService>();
            
            var instrumentService = provider.GetRequiredService<IInstrumentService>();

            
            var token = await logInService.LogInAsync(Dto.CreateUser());

            var someInstruments = await instrumentService.GetInstrument(1, 20);
            
            var someAlerts = await alertService.GetAlert(1, 20);

            var someCreatedAlert = await alertService.CreateAlert(Dto.CreateAlert());
            
            HubConnection = new HubConnectionBuilder()
                .WithUrl(Api.BaseUrl + "/ticks-hub", options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(token.Token);
                })
                .Build();
            
            HubConnection.On<Tick>("TR1011", (tick) =>
            {
                Console.WriteLine($"Notification {tick.Symbol}");
            });
            await HubConnection.StartAsync();
            await HubConnection.SendAsync("subscribeToTicks", "TR1011");

            
            foreach (var i in someInstruments.Data)
            {
                Console.WriteLine(i.Symbol);
            }

            Console.ReadLine();
        }
    }
}