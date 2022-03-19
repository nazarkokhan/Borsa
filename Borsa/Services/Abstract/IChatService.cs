namespace Borsa.Services.Abstract;

using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public interface IChatService
{
    public Task<ChatMember> GetMyProfile();

    public Task<GetChatDto?> GetChat(int id, int messagesCount);
}

public class ChatService : IChatService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _serializerOptions;

    public ChatService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<ChatMember> GetMyProfile()
    {
        var response = await _httpClient
            .GetAsync("User");

        if (response.StatusCode == HttpStatusCode.BadRequest)
            return null!;

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        var me = JsonSerializer.Deserialize<ChatMember>(content, _serializerOptions);

        return me!;
    }

    public async Task<GetChatDto?> GetChat(int id, int messagesCount)
    {
        var response = await _httpClient
            .GetAsync($"ChatV2?{nameof(id)}={id}&{nameof(messagesCount)}={messagesCount}");

        if (response.StatusCode == HttpStatusCode.BadRequest)
            return null!;

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        var chat = JsonSerializer.Deserialize<GetChatDto>(content, _serializerOptions);

        return chat;
    }
}