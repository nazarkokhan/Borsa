using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public record NewMessageDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("body")] string Body,
    [property: JsonPropertyName("createdDate")]
    DateTime CreatedDate,
    [property: JsonPropertyName("chatId")] int ChatId,
    [property: JsonPropertyName("userId")] int UserId);

public record UpdateMessageDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("body")] string Body,
    [property: JsonPropertyName("changedDate")]
    DateTime ChangedDate,
    [property: JsonPropertyName("chatId")] int ChatId,
    [property: JsonPropertyName("userId")] int UserId);

public record ReadByMessagesDto(
    [property: JsonPropertyName("messageIds")]
    ICollection<Guid> MessageIds,
    [property: JsonPropertyName("chatId")] int ChatId,
    [property: JsonPropertyName("userId")] int UserId);