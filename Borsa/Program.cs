using System;
using System.Collections.Generic;
using Borsa;

// ReSharper disable All

namespace Borsa
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Constants;
    using Constants.Url;
    using DTO.Authorization;
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
                .AddSingleton<IChatService, ChatService>()
                .AddScoped<AuthInterceptor>()
                .AddSingleton<IConfiguration>(configuration)
                .AddHttpClient(
                    nameof(LoginService),
                    client => { client.BaseAddress = new Uri(Api.BaseUrl + "api/"); }
                )
                .Services
                .AddHttpClient<IChatService, ChatService>(
                    client => { client.BaseAddress = new Uri(Api.BaseUrl + "api/"); }
                )
                .AddHttpMessageHandler<AuthInterceptor>()
                .Services
                .BuildServiceProvider();

            var logInService = provider.GetRequiredService<ILoginService>();

            var chatService = provider.GetRequiredService<IChatService>();

            var token = await logInService.LogInAsync(new LogInQuery(
                "superadmin@gmail.com",
                "Admin123"));

            HubConnection = new HubConnectionBuilder()
                .WithUrl(Api.BaseUrl + "hub",
                    options => { options.AccessTokenProvider = () => Task.FromResult(token.Token); })
                .Build();

            var me = await chatService.GetMyProfile();

            var chats = new List<GetChatDto>();

            HubConnection.On<NewMessageDto>(
                "ReceiveNewMessage",
                async (newMessage) =>
                {
                    var message = newMessage.ToMessage();

                    var consoleMessage = "#####(New message)#####\n";

                    var chat = chats.Find(c => c.Id == message.ChatId);

                    if (chat is null)
                    {
                        chat = await chatService.GetChat(message.ChatId, 10);

                        if (chat is null)
                        {
                            Console.WriteLine($"LOG ERROR: Chat with id: {message.ChatId} not found");

                            return;
                        }

                        chats.Add(chat);
                    }

                    var iAmOwner = message.UserId == me.Id;

                    var messageOwner = iAmOwner
                        ? me
                        : chat.ChatMembers
                            .First(m => m.Id == message.UserId);

                    consoleMessage += message.ToDisplayText(messageOwner, iAmOwner);

                    Console.WriteLine(consoleMessage);
                });

            HubConnection.On<UpdateMessageDto>(
                "ReceiveUpdateMessage",
                (updateMessage) =>
                {
                    // var message = newMessage.ToMessage();
                    //
                    // var consoleMessage = "New message\n";
                    //
                    // consoleMessage += message.ToDisplayText();
                    //
                    // Console.WriteLine(consoleMessage);
                });

            HubConnection.On<ReadByMessagesDto>(
                "ReceiveReadByMessages",
                (readByMessages) =>
                {
                    // var message = newMessage.ToMessage();
                    //
                    // var consoleMessage = "New message\n";
                    //
                    // consoleMessage += message.ToDisplayText();
                    //
                    // Console.WriteLine(consoleMessage);
                });

            await HubConnection.StartAsync();

            while (true)
            {
                Console.WriteLine("Action type:");
                var methodName = Console.ReadLine();
                Console.WriteLine("----------");

                Console.WriteLine("ChatId:");
                var chatId = int.Parse(Console.ReadLine()!);
                Console.WriteLine("----------");

                string? message;
                switch (methodName)
                {
                    case "New":
                        Console.WriteLine("Write message:");
                        message = Console.ReadLine();
                        Console.WriteLine("----------");

                        await HubConnection.SendAsync(
                            "SendNewMessage",
                            chatId,
                            message);
                        break;

                    case "Update":
                        Console.WriteLine("Write message id:");
                        var messageId = Guid.Parse(Console.ReadLine()!);
                        Console.WriteLine("----------");

                        Console.WriteLine("Write message:");
                        message = Console.ReadLine();
                        Console.WriteLine("----------");

                        await HubConnection.SendAsync(
                            "SendUpdateMessage",
                            chatId,
                            messageId,
                            message);
                        break;

                    case "Read":
                        var messageIds = Console.ReadLine()!
                            .Split(", ")
                            .Select(id => Guid.Parse(id))
                            .ToList();

                        await HubConnection.SendAsync(
                            "SendReadMessages",
                            chatId,
                            messageIds);
                        break;

                    default:
                        break;
                }

                await Task.Delay(300);
            }
        }
    }
}

public static class DisplayConsole
{
    public static string ToDisplayText(this ChatMember m, bool iAmOwner)
    {
        return $"--{nameof(iAmOwner).ToUpper()}: {iAmOwner}\n" +
               $"-{nameof(m.Id)}: {m.Id}\n" +
               $"-{nameof(m.FirstName)}: {m.FirstName}\n" +
               $"-{nameof(m.LastName)}: {m.LastName}\n" +
               $"-{nameof(m.Role)}: {m.Role}\n";
    }

    public static string ToDisplayText(this Message m, ChatMember messageOwner, bool iAmOwner)
    {
        return "----------------------------\n" +
               $"Message owner:\n {messageOwner.ToDisplayText(iAmOwner)}\n" +
               $"{nameof(m.Id)}: {m.Id}\n" +
               $"{nameof(m.Body)}: {m.Body}\n" +
               $"{nameof(m.IsRead)}: {m.IsRead}\n" +
               $"{nameof(m.CreatedDate)}: {m.CreatedDate}\n" +
               $"{nameof(m.ChangedDate)}: {m.ChangedDate}\n" +
               $"{nameof(m.ChatId)}: {m.ChatId}\n" +
               $"{nameof(m.UserId)}: {m.UserId}\n" +
               "----------------------------\n";
    }
}

public static class Mappers
{
    public static Message ToMessage(this NewMessageDto newMessageDto)
    {
        return newMessageDto.Map(m => new Message(
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