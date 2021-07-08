using System.Text.Json.Serialization;
using Borsa.DTO.Enums;

namespace Borsa.DTO.Alert
{
    public class ConditionDto
    {
        public ConditionDto(CompareType compareType, ExpressionDto leftExpression, 
            ExpressionDto rightExpression, InstrumentDto instrument)
        {
            CompareType = compareType;
            LeftExpression = leftExpression;
            RightExpression = rightExpression;
            Instrument = instrument;
        }

        [JsonPropertyName("compareType")]
        public CompareType CompareType { get; }

        [JsonPropertyName("leftExpression")]
        public ExpressionDto LeftExpression { get; }

        [JsonPropertyName("rightExpression")]
        public ExpressionDto RightExpression { get; }

        [JsonPropertyName("instrument")]
        public InstrumentDto Instrument { get; }
    }
}