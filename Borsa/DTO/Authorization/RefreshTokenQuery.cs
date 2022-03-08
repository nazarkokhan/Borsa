using System.Text.Json.Serialization;

namespace Borsa.DTO.Authorization;

public class RefreshTokenQuery
{
    public RefreshTokenQuery(string refreshToken)
    {
        RefreshToken = refreshToken;
    }

    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; }
}