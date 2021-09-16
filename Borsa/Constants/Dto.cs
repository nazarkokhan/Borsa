using System.Collections.Generic;
using Borsa.DTO;
using Borsa.DTO.Alert.Create;
using Borsa.DTO.Authorization;
using Borsa.DTO.Enums;

namespace Borsa.Constants
{
    public static class Dto
    {
        public static LogInQuery CreateUser()
        {
            return new LogInQuery(
                LoginData.Email,
                LoginData.Password
            );
        }

        public static CreateAlertDto CreateAlert()
        {
            return new CreateAlertDto(
                conditions: new List<CreateConditionDto>
                {
                    new CreateConditionDto
                    (
                        compareType: CompareType.BiggerThen,
                        leftExpression: new CreateExpressionDto
                        (
                            indicatorType: IndicatorType.Price,
                            parameters: new Dictionary<string, string> { }
                        ),
                        rightExpression: new CreateExpressionDto
                        (
                            IndicatorType.Number,
                            new Dictionary<string, string>
                            {
                                {IndicatorType.Number.ToString(), 100.ToString()},
                            }
                        ),
                        instrumentId: 3842
                    )
                },
                name: "Alert",
                notes: "My Notes",
                buySell: BuySell.Buy,
                autoStart: true
            );
        }
    }
}