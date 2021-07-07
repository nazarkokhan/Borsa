using System.Text.Json.Serialization;
using Borsa.DTO.Enums;

namespace Borsa.DTO.Alert.Create
{
    public class CreateConditionDto
    {
        public CreateConditionDto(
            CompareType compareType, CreateExpressionDto leftExpression, 
            CreateExpressionDto rightExpression, int instrumentId)
        {
            CompareType = compareType;
            LeftExpression = leftExpression;
            RightExpression = rightExpression;
            InstrumentId = instrumentId;
        }
        
        [JsonPropertyName("compareType")]
        public CompareType CompareType { get; }

        [JsonPropertyName("leftExpression")]
        public CreateExpressionDto LeftExpression { get; }
        
        [JsonPropertyName("rightExpression")]
        public CreateExpressionDto RightExpression { get; }
        
        [JsonPropertyName("instrumentId")]
        public int InstrumentId { get; }
    }
}