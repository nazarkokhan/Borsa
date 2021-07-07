using System;

namespace Borsa.DTO.Enums
{
    [Flags]
    public enum IndicatorType
    {
        Price = 1,
        Volume = 2,
        Number = 4
    }
}