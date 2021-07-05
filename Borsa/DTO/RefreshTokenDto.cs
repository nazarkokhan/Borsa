using System.Text.Json.Serialization;

namespace Borsa.DTO
{
    public class RefreshTokenDto
    {
        public RefreshTokenDto(string refreshToken)
        {
            RefreshToken = refreshToken;
        }

        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; }
    }
}