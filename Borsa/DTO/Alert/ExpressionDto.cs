﻿using System.Collections.Generic;
using System.Text.Json.Serialization;
using Borsa.DTO.Enums;

namespace Borsa.DTO.Alert;

public class ExpressionDto
{
    public ExpressionDto(IndicatorType indicatorType, Dictionary<string, string> parameters)
    {
        IndicatorType = indicatorType;
        Parameters = parameters;
    }

    [JsonPropertyName("indicatorType")]
    public IndicatorType IndicatorType { get; }

    [JsonPropertyName("parameters")]
    public Dictionary<string, string> Parameters { get; }
}