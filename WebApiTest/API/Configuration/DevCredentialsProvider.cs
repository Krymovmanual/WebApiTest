using Newtonsoft.Json;

namespace WebApiTest.API.Configuration;

public static class DevCredentialsProvider
{
    private const string UserNameEnvironmentVariable = "DEV_USERNAME";
    private const string PasswordEnvironmentVariable = "DEV_PASSWORD";
    private const string LocalCredentialsFileName = "dev_creds.json";

    public static DevCredentials GetCredentials()
    {
        var userName = Environment.GetEnvironmentVariable(UserNameEnvironmentVariable);
        var password = Environment.GetEnvironmentVariable(PasswordEnvironmentVariable);

        if (!string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(password))
        {
            return new DevCredentials
            {
                UserName = userName,
                Password = password
            };
        }

        var credentialsFile = FindCredentialsFile();
        if (credentialsFile is null)
        {
            throw new InvalidOperationException(
                $"Dev credentials were not found. Set {UserNameEnvironmentVariable} and {PasswordEnvironmentVariable}, " +
                $"or create {LocalCredentialsFileName} in the repository root.");
        }

        var json = File.ReadAllText(credentialsFile);
        var credentials = JsonConvert.DeserializeObject<DevCredentials>(json);

        if (credentials is null ||
            string.IsNullOrWhiteSpace(credentials.UserName) ||
            string.IsNullOrWhiteSpace(credentials.Password))
        {
            throw new InvalidOperationException(
                $"{LocalCredentialsFileName} must contain non-empty userName and password values.");
        }

        return credentials;
    }

    private static string? FindCredentialsFile()
    {
        return FindCredentialsFileFrom(Directory.GetCurrentDirectory()) ??
               FindCredentialsFileFrom(AppContext.BaseDirectory);
    }

    private static string? FindCredentialsFileFrom(string startDirectory)
    {
        var directory = new DirectoryInfo(startDirectory);

        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, LocalCredentialsFileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return null;
    }
}
