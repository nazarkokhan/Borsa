﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    using Microsoft.Extensions.Logging;
    using Services;
    using Services.Abstract;

    public record NotificationBanner(
        string Title,
        string Body,
        Dictionary<string, string>? Data);
    
    internal static partial class Program
    {
        public static async Task<GetChatDto?> GetChatCached(this IChatService chatService, int chatId)
        {
            var chat = chatList.FirstOrDefault(c => c.Id == chatId);

            if (chat is null)
            {
                chat = await chatService.GetChat(chatId, 3);

                if (chat is null)
                {
                    Console.WriteLine($"LOG ERROR: Chat with id: {chatId} not found");

                    return null;
                }

                chat.ChatMembers.Add(myUser);

                chatList.Add(chat);
            }

            return chat;
        }

        private static List<GetChatDto> chatList = new List<GetChatDto>();
        private static ChatMember myUser = null!;
        private static HubConnection HubConnection { get; set; } = null!;
        private static ILogger _logger = null!;
        private static int totalUnreadMessagesCount = 0;

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
                    client => { client.BaseAddress = new Uri("http://localhost:7000/"); })
                .Services
                .AddHttpClient<IChatService, ChatService>(
                    client => { client.BaseAddress = new Uri("http://localhost:5120/"); })
                .AddHttpMessageHandler<AuthInterceptor>()
                .Services
                .BuildServiceProvider();

            var logInService = provider.GetRequiredService<ILoginService>();

            var chatService = provider.GetRequiredService<IChatService>();

            var token = await logInService.LogInAsync(new LogInQuery(
                "+380983206870",
                "000000",
                "string",
                "string",
                "string"));

            HubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5120/finalto-quotes",
                    options => { options.AccessTokenProvider = () => Task.FromResult(token.Token); })
                .WithAutomaticReconnect()
                .Build();

            // myUser = await chatService.GetMyProfile();

            HubConnection.On<NotificationBanner>(
                "OnNotificationBanner",
                (newMessage) =>
                {
                    Console.WriteLine(newMessage.ToString());
                });
            
            HubConnection.Reconnected += connectionId =>
            {
                Console.WriteLine($"Connection successfully reconnected. The ConnectionId is now: {connectionId}");

                return Task.CompletedTask;
            };

            await HubConnection.StartAsync();

            while (true)
            {
                // await HubConnection.SendCoreAsync("Do", new []{"fak"});
                await Task.Delay(1000);
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
               $"I am owner: {(iAmOwner ? "+YES+" : "-NO-")}\n" +
               $"{nameof(m.Id)}: {m.Id}\n" +
               $"{nameof(m.Body)}: {m.Body}\n" +
               $"{nameof(m.IsRead)}: {m.IsRead}\n" +
               $"{nameof(m.CreatedDate)}: {m.CreatedDate}\n" +
               $"{nameof(m.ChangedDate)}: {m.ChangedDate}\n" +
               $"{nameof(m.UserId)}: {m.UserId}\n" +
               "______________MESSAGE_______________\n";
    }
}

public static class Mappers
{
    public static Message MapToMessage(this NewMessageDto newMessageDto)
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