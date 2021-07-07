using System;

namespace Borsa.DTO.Enums
{
    [Flags]
    public enum CompareType
    {
        BiggerThenOrEqual = 1, 
        LessThenOrEqual = 2, 
        BiggerThen = 4, 
        LessThen = 8
    }
}