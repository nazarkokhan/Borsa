using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Borsa.DTO.Enums;

namespace Borsa.DTO.Alert
{
    public class AlertDto
    {
        public AlertDto(int id, string name, DateTime createTime, 
            string notes, ActivityStatus status, int userId, 
            List<ConditionDto> conditions, BuySell buySell)
        {
            Id = id;
            Name = name;
            CreateTime = createTime;
            Notes = notes;
            Status = status;
            UserId = userId;
            Conditions = conditions;
            BuySell = buySell;
        }

        [JsonPropertyName("id")]
        public int Id { get; }

        [JsonPropertyName("name")]
        public string Name { get; }

        [JsonPropertyName("createTime")]
        public DateTime CreateTime { get; }

        [JsonPropertyName("notes")]
        public string Notes { get; }

        [JsonPropertyName("status")]
        public ActivityStatus Status { get; }

        [JsonPropertyName("userId")]
        public int UserId { get; }

        [JsonPropertyName("conditions")]
        public List<ConditionDto> Conditions { get; }
        
        [JsonPropertyName("buySell")]
        public BuySell BuySell { get; }
    }
}