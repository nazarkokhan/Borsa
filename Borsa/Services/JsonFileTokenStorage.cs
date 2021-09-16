using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Borsa.DTO.Authorization;
using Borsa.Services.Abstract;

namespace Borsa.Services
{
    public class JsonFileTokenStorage : ITokenStorage
    {
        private const string FileName = "tokenStorage.json";

        private readonly string _path;
        private LogInQueryResult _logInQueryResult;

        public JsonFileTokenStorage()
        {
            _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName);
        }

        public async Task<LogInQueryResult> GetToken()
        {
            if (_logInQueryResult is not null)
            {
                return _logInQueryResult;
            }

            if (File.Exists(_path))
            {
                return _logInQueryResult = JsonSerializer.Deserialize<LogInQueryResult>(
                    await File.ReadAllTextAsync(_path)
                );
            }

            await SaveToken(new LogInQueryResult(
                "DEFAULT_TOKEN",
                DateTime.UnixEpoch,
                "DEFAULT_REFRESH_TOKEN",
                DateTime.UnixEpoch)
            );

            return _logInQueryResult;
        }

        public async Task SaveToken(LogInQueryResult logInQueryResult)
        {
            await File.WriteAllTextAsync(_path, JsonSerializer.Serialize(logInQueryResult));
            _logInQueryResult = logInQueryResult;
        }
    }
}