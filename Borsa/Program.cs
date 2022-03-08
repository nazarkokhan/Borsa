using System;
using System.Collections.Generic;
// ReSharper disable All

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
                    client => { client.BaseAddress = new Uri(Api.BaseUrl + "/api"); }
                )
                .Services
                .AddHttpClient(
                    Client.AuthClient,
                    client => { client.BaseAddress = new Uri(Api.BaseUrl + "/api"); }
                )
                .AddHttpMessageHandler<AuthInterceptor>()
                .Services
                .BuildServiceProvider();

            var logInService = provider.GetRequiredService<ILoginService>();

            var token = await logInService.LogInAsync(Dto.CreateUser());

            HubConnection = new HubConnectionBuilder()
                .WithUrl(Api.BaseUrl + "hub",
                    options => { options.AccessTokenProvider = () => Task.FromResult(token.Token); })
                .Build();

            HubConnection.On<NewMessageDto>(
                "ReceiveNewMessage",
                (newMessage) =>
                {
                    var m = newMessage.ToDisplayMessageDto();

                    var consoleMessage = "New message\n";

                    consoleMessage += $"{nameof(m.Id)}: {m.Id}\n" +
                                      $"{nameof(m.Body)}: {m.Body}\n" +
                                      $"{nameof(m.IsRead)}: {m.IsRead}\n" +
                                      $"{nameof(m.CreatedDate)}: {m.CreatedDate}\n" +
                                      $"{nameof(m.ChangedDate)}: {m.ChangedDate}\n" +
                                      $"{nameof(m.ChatId)}: {m.ChatId}\n" +
                                      $"{nameof(m.UserId)}: {m.UserId}\n";

                    Console.WriteLine(consoleMessage);
                });

            await HubConnection.StartAsync();

            const int integerChatId = 9;

            Console.WriteLine("Write message:");

            while (true)
            {
                await HubConnection.SendAsync(
                    "SendMessage",
                    integerChatId,
                    Console.ReadLine());
            }
        }
    }
}

public static class Mappers
{
    public static DisplayMessage ToDisplayMessageDto(this NewMessageDto newMessageDto)
    {
        return newMessageDto.Map(m => new DisplayMessage(
            m.Id,
            m.Body,
            false,
            m.CreatedDate,
            null,
            m.ChatId,
            m.UserId));
    }

    public static TResult Map<TSource, TResult>(this TSource source, Func<TSource, TResult> func)
        => func.Invoke(source);
}