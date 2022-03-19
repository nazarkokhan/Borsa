namespace Borsa;

using System;
using System.Collections.Generic;

public record GetChatDto(
    int Id,
    ChatType Type,
    DateTime LastActionDate,
    DateTime CreatedDate,
    List<ChatMember> ChatMembers,
    List<Message> Messages,
    int UnreadCount)
{
    public List<Message> Messages { get; set; } = Messages;
}

public record ChatMember(
    int Id,
    string? FirstName,
    string? LastName,
    string? AvatarUrl,
    string Role);

public record Message(
    Guid Id,
    string Body,
    bool IsRead,
    DateTime CreatedDate,
    DateTime? ChangedDate,
    int ChatId,
    int UserId);

public enum ChatType
{
    OneToOne = 1,
    Support = 2,
    Group = 3
}