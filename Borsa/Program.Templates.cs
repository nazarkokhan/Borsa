namespace Borsa;

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Services.Abstract;

internal static partial class Program
{
    public static void Templates()
    {
        IChatService chatService = null!;

        ChatMember myUser = null!;

        var chatList = new List<GetChatDto>();

        GetChatDto? currentOpenedChat = null;

        var userIsOnMessagesTab = false;

        HubConnection.On<NewMessageDto>(
            "ReceiveNewMessageTemplate",
            async (newMessage) =>
            {
                totalUnreadMessagesCount++;

                if (!userIsOnMessagesTab)
                    return;

                var chatId = newMessage.ChatId;

                bool messageIsNewer;

                if (currentOpenedChat?.Id == chatId)
                {
                    messageIsNewer = newMessage.CreatedDate > currentOpenedChat.Messages.Last().CreatedDate;

                    currentOpenedChat.AddMessage(newMessage, myUser.Id);

                    if (!messageIsNewer)
                        currentOpenedChat.Messages = currentOpenedChat.Messages
                            .OrderByDescending(m => m.CreatedDate)
                            .ToList();
                }

                var chatFromList = chatList.Find(c => c.Id == chatId);

                if (chatFromList is null)
                {
                    //Api request GET api/Chat, move received chat on top
                    chatFromList = await chatService.GetChat(chatId, messagesCount: 10);

                    //That should not happen but...
                    if (chatFromList is null) //Actually if chat not found you will receive error "CHAT_NOT_FOUND"
                    {
                        _logger.LogWarning("Received message to chatId: {ChatId} that was not found", chatId);
                        return;
                    }

                    chatList.Add(chatFromList);
                }
     
                // move chat on top of the chat list
                chatFromList.AddMessage(newMessage, myUser.Id);
            });

        HubConnection.On<UpdateMessageDto>(
            "ReceiveUpdateMessageTemplate",
            (updateMessage) =>
            {
                if (!userIsOnMessagesTab)
                    return;

                var chatId = updateMessage.ChatId;

                var chatToUpdate = currentOpenedChat?.Id == chatId
                    ? currentOpenedChat
                    : chatList.Find(c => c.Id == chatId);

                if (chatToUpdate is null)
                {
                    _logger.LogInformation(
                        "Chat({ChatId}) where need to update message({MessageId}) is not found on page",
                        chatId, updateMessage.Id);

                    return;
                }

                var message = chatToUpdate.Messages
                    .Find(m => m.Id == updateMessage.Id);

                if (message is null)
                {
                    _logger.LogInformation("Message({MessageId}) to update in chat({ChatId}) was not found",
                        updateMessage.Id, chatId);

                    return;
                }

                message = message with
                {
                    Body = updateMessage.Body,
                    ChangedDate = updateMessage.ChangedDate
                };
            });

        HubConnection.On<ReadByMessagesDto>(
            "ReceiveReadByMessagesTemplate",
            (readByMessages) =>
            {
                if (!userIsOnMessagesTab)
                    return;

                var chatId = readByMessages.ChatId;

                var chatToRead = currentOpenedChat?.Id == chatId
                    ? currentOpenedChat
                    : chatList.Find(c => c.Id == chatId);

                if (chatToRead is null)
                {
                    _logger.LogInformation("Chat({ChatId}) where need to read messages is not found on page",
                        chatId);

                    return;
                }

                foreach (var messageToReadId in readByMessages.MessageIds)
                {
                    var message = chatToRead.Messages
                        .Find(m => m.Id == messageToReadId);

                    if (message is null)
                    {
                        _logger.LogInformation("Message({MessageId}) to read in chat({ChatId}) was not found",
                            messageToReadId, chatId);

                        return;
                    }

                    var readerId = readByMessages.UserId;

                    var messageIsMy = message.UserId == myUser.Id;

                    var iAmReader = readerId == myUser.Id;

                    if (messageIsMy && iAmReader)
                    {
                        _logger.LogWarning("User cant read his own message({MessageId})", messageToReadId);

                        return;
                    }

                    //It is important check for chats that have more than 2 users
                    //This happens when you 1 user read message of user 2.
                    //All users in chat receive that this action happened
                    //but user 3 should not care about this action
                    if (!messageIsMy && !iAmReader)
                    {
                        _logger.LogInformation(
                            "Not my message({MessageId}) with owner({Owner}) was read by other user({ReaderId})",
                            messageToReadId, message.UserId, readerId);

                        return;
                    }

                    if (message.IsRead)
                    {
                        _logger.LogInformation("Message({MessageId}) is already read", messageToReadId);

                        return;
                    }

                    message = message with
                    {
                        IsRead = true
                    };
                }
            });
    }

    public static void AddMessage(this GetChatDto chat, NewMessageDto message, int myUserId)
    {
        var messageIsMineOrOthersMember = message.UserId == myUserId ||
                                          chat.ChatMembers
                                              .Any(m => m.Id == message.UserId);

        //That should not happen
        if (!messageIsMineOrOthersMember)
        {
            totalUnreadMessagesCount--;
            _logger.LogWarning("Message owner({MessageOwner}) was not found in chat ({ChatId})",
                message.UserId, chat.Id);
        }

        chat.Messages.Add(message.MapToMessage());

        // ReSharper disable once RedundantAssignment
        chat = chat with
        {
            LastActionDate = message.CreatedDate
        };
    }
}