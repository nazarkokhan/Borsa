using System;
using Borsa.DTO.Authorization;

namespace Borsa.Extensions;

public static class Extension
{
    public static bool IsExpired(this LogInQueryResult logInQueryResult)
    {
        return DateTime.UtcNow >= logInQueryResult.TokenExpTime;
    }
}