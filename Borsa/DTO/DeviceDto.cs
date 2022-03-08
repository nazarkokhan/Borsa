namespace Borsa.DTO;

public class DeviceDto
{
    public DeviceDto(string token, string os)
    {
        Token = token;
        OS = os;
    }

    public string Token { get; }

    public string OS { get; }
}