using System;
using System.Text.Json.Serialization;

namespace Borsa
{
    public class Message
    {
        public Message(
            int chatId, 
            int userId, 
            string body, 
            DateTime createdDate)
        {
            ChatId = chatId;
            UserId = userId;
            Body = body;
            CreatedDate = createdDate;
        }

        [JsonPropertyName("chatId")]
        public int ChatId { get; }
        
        [JsonPropertyName("userId")]
        public int UserId { get; }

        [JsonPropertyName("body")]
        public string Body { get; }
        
        [JsonPropertyName("createdDate")]
        public DateTime CreatedDate { get; }
    }
}