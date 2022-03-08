namespace Borsa.Services.Abstract;

using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public interface IChatService
{
    public Task<GetChatDto> GetChat(int id, int messagesCount);
}

public class ChatService : IChatService
{
    private readonly HttpClient _httpClient;

    public ChatService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GetChatDto> GetChat(int id, int messagesCount)
    {
        var response = await _httpClient
            .GetAsync($"ChatV2?{nameof(id)}={id}&{nameof(messagesCount)}=?{messagesCount}");

        if (response.StatusCode == HttpStatusCode.BadRequest)
            return null!;
            
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        
        var chat = JsonSerializer.Deserialize<GetChatDto>(content);

        return chat!;
    }
}