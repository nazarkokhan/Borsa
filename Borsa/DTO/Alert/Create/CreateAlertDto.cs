using System.Collections.Generic;
using System.Text.Json.Serialization;
using Borsa.DTO.Enums;

namespace Borsa.DTO.Alert.Create;

public class CreateAlertDto
{
    public CreateAlertDto(
        List<CreateConditionDto> conditions, string name, 
        string notes, BuySell buySell, bool autoStart)
    {
        Conditions = conditions;
        Name = name;
        Notes = notes;
        BuySell = buySell;
        AutoStart = autoStart;
    }
        
    [JsonPropertyName("conditions")]
    public List<CreateConditionDto> Conditions { get; }

    [JsonPropertyName("name")]
    public string Name { get;  }

    [JsonPropertyName("notes")]
    public string Notes { get;  }
        
    [JsonPropertyName("buySell")]
    public BuySell BuySell { get; }

    [JsonPropertyName("autoStart")]
    public bool AutoStart { get; }
}