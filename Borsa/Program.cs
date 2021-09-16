namespace Borsa
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Constants;
    using Constants.Url;
    using Extensions;
    using Microsoft.AspNetCore.SignalR.Client;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Services;
    using Services.Abstract;

    internal static class Program
    {
        private static HubConnection HubConnection { get; set; }

        private static async Task Main()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, FileName.AppSettings
                ))
                .Build();

            var provider = new ServiceCollection()
                .AddScoped<ILoginService, LoginService>()
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

            var token = await logInService.LogInAsync(Dto.CreateUser());

            HubConnection = new HubConnectionBuilder()
                .WithUrl(Api.BaseUrl + "/Chat",
                    options => { options.AccessTokenProvider = () => Task.FromResult(token.Token); })
                .Build();

            HubConnection.On<MessageDto>(
                "Send",
                (message) =>
                {
                    Console.WriteLine(
                        $"UserId: {message.UserId} /// ChatId: {message.ChatId}\n" +
                        $"Message: {message.Body}\n" +
                        $"CreatedDate: {message.CreatedDate}\n"
                    );
                }
            );

            await HubConnection.StartAsync();

            Console.WriteLine("WriteChatId");

            var chatId = Console.ReadLine();

            while (true)
            {
                await HubConnection.SendAsync(
                    "SendMessage",
                    chatId,
                    Console.ReadLine(),
                    CancellationToken.None
                );
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
}