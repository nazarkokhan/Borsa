using System;
using System.Collections.Generic;

public record DisplayMessage(
    Guid Id,
    string Body,
    bool IsRead,
    DateTime CreatedDate,
    DateTime? ChangedDate,
    int ChatId,
    int UserId);
    
public record NewMessageDto(
    Guid Id,
    string Body,
    DateTime CreatedDate,
    int ChatId,
    int UserId);

public record UpdateMessageDto(
    Guid Id,
    string Body,
    DateTime ChangedDate,
    int ChatId,
    int UserId);

public record MessagesReadByDto(
    ICollection<Guid> MessageIds,
    int ChatId,
    int UserId);