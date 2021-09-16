namespace Borsa.DTO.Authorization
{
    public class LogInQuery
    {
        public LogInQuery(
            string email, 
            string password)
        {
            Email = email;
            Password = password;
        }

        public string Email { get; }

        public string Password { get; }
    }
}