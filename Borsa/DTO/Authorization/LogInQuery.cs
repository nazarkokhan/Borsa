namespace Borsa.DTO.Authorization;

public record LogInQuery(
    string Username, 
    string Password,
    string Code,
    string DeviceId,
    string DeviceToken);