using System;
using System.Collections.Generic;
using System.Linq;
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
                .WithAutomaticReconnect()
                .Build();

            var me = await chatService.GetMyProfile();

            var chats = new List<GetChatDto>();

            HubConnection.On<NewMessageDto>(
                "ReceiveNewMessage",
                async (newMessage) =>
                {
                    var message = newMessage.ToMessage();
            
                    var chat = chats.FirstOrDefault(c => c.Id == message.ChatId);
            
                    if (chat is null)
                    {
                        chat = await chatService.GetChat(message.ChatId, 3);
            
                        if (chat is null)
                        {
                            Console.WriteLine($"LOG ERROR: Chat with id: {message.ChatId} not found");
            
                            return;
                        }
            
                        chat.ChatMembers.Add(me);
            
                        chats.Add(chat);
                    }
            
                    chat.Messages.Add(message);
            
                    Console.Clear();
            
                    Console.WriteLine(chat.ToDisplayText(me.Id));
            
                    Console.WriteLine($"#####(New message. Id: {newMessage.Id})#####\n");
                });

            HubConnection.On<UpdateMessageDto>(
                "ReceiveUpdateMessage",
                (updateMessage) =>
                {
                    var chat = chats.First(c => c.Id == updateMessage.ChatId);

                    if (chat is null)
                    {
                        Console.WriteLine("___________________");
                        Console.WriteLine("CHAT FOR UPDATE MESSAGE NOT FOUND");
                        Console.WriteLine("___________________");

                        return;
                    }

                    var message = chat.Messages
                        .First(m => m.Id == updateMessage.Id &&
                                    m.UserId == updateMessage.UserId);

                    message = new Message(
                        message.Id,
                        updateMessage.Body,
                        message.IsRead,
                        message.CreatedDate,
                        updateMessage.ChangedDate,
                        updateMessage.ChatId,
                        updateMessage.UserId);

                    chat.Messages.RemoveAll(x => x.Id == message.Id);

                    chat.Messages.Add(message);

                    Console.Clear();

                    Console.WriteLine(chat.ToDisplayText(me.Id));

                    Console.WriteLine($"#####(Updated message. Id: {updateMessage.Id})#####\n");
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

                    Console.Clear();

                    var reads = readByMessages.MessageIds
                        .Select(id => id.ToString())
                        .JoinToSingle("\n_-_: ");

                    Console.WriteLine($"#####(Read by messages. Ids:\n {reads})#####\n");
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
                        Console.WriteLine("WRONG METHOD");
                        break;
                }

                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
    }
}

public static class DisplayConsole
{
    public static string ToDisplayText(this GetChatDto c, int meId)
    {
        var chat = "______________CHAT_______________\n" +
                   $"-{nameof(c.Id)}: {c.Id}\n" +
                   $"-{nameof(c.CreatedDate)}: {c.CreatedDate}\n" +
                   $"-{nameof(c.LastActionDate)}: {c.LastActionDate}\n" +
                   $"-{nameof(c.UnreadCount)}: {c.UnreadCount}\n" +
                   "______________CHAT_______________\n";

        var members = c.ChatMembers
            .OrderByDescending(m => m.Id == meId)
            .Select(m => m.ToDisplayText(m.Id == meId))
            .JoinToSingle("\n<<<<<<<<<<_next_>>>>>>>>>\n");

        var messages = c.Messages
            .Select(m => m.ToDisplayText(
                c.ChatMembers
                    .First(cm => cm.Id == m.UserId),
                m.UserId == meId))
            .JoinToSingle("\n<<<<<<<<<<_next_>>>>>>>>>\n");

        return $"________START________\n\n" +
               $"{chat}\n" +
               $"_____**********_____\n" +
               $"{members}\n" +
               $"_____**********_____\n" +
               $"{messages}\n" +
               $"________END________\n\n";
    }

    public static string ToDisplayText(this ChatMember m, bool iAmOwner)
    {
        return "______________MEMBER_______________\n" +
               $"--IT IS ME: {iAmOwner}\n" +
               $"-{nameof(m.Id)}: {m.Id}\n" +
               $"-{nameof(m.FirstName)}: {m.FirstName}\n" +
               $"-{nameof(m.LastName)}: {m.LastName}\n" +
               $"-{nameof(m.Role)}: {m.Role}\n" +
               "______________MEMBER________________\n";
    }

    public static string ToDisplayText(this Message m, ChatMember messageOwner, bool iAmOwner)
    {
        return "______________MESSAGE_______________\n" +
               // $"Message owner:\n {messageOwner.ToDisplayText(iAmOwner)}\n" +
               $"I AM OWNER: {(iAmOwner ? "+YES+" : "-NO-")}\n" +
               $"{nameof(m.Id)}: {m.Id}\n" +
               $"{nameof(m.Body)}: {m.Body}\n" +
               $"{nameof(m.IsRead)}: {m.IsRead}\n" +
               $"{nameof(m.CreatedDate)}: {m.CreatedDate}\n" +
               $"{nameof(m.ChangedDate)}: {m.ChangedDate}\n" +
               $"{nameof(m.ChatId)}: {m.ChatId}\n" +
               $"{nameof(m.UserId)}: {m.UserId}\n" +
               "______________MESSAGE_______________\n";
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

    public static string JoinToSingle(this IEnumerable<string> strings, string separator)
        => string.Join(separator, strings);
}