using System;
using System.Collections.Generic;

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

public record ReadByMessagesDto(
    ICollection<Guid> MessageIds,
    int ChatId,
    int UserId);