using System;
using Borsa.DTO.Authorization;

namespace Borsa.Extensions
{
    public static class Extension
    {
        public static bool IsExpired(this TokenDto tokenDto)
        {
            return DateTime.UtcNow >= tokenDto.TokenExpTime;
        }
    }
}