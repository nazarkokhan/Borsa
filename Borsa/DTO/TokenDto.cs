using System.Text.Json.Serialization;

namespace Borsa.DTO
{
    public class TokenDto
    {
        public TokenDto(string token, string refreshToken)
        {
            Token = token;
            RefreshToken = refreshToken;
        }

        [JsonPropertyName("token")]
        public string Token { get; }
        
        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; }
    }
}