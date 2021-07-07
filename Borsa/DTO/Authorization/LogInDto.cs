namespace Borsa.DTO.Authorization
{
    public class LogInDto
    {
        public LogInDto(string email, string password, DeviceDto device)
        {
            Email = email;
            Password = password;
            Device = device;
        }

        public string Email { get; }

        public string Password { get; }

        public DeviceDto Device { get; }
    }
}