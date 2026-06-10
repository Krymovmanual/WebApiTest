namespace WebApiTest.API.Authorization;

public sealed class StageTokenRequest
{
    public string GrantType { get; init; } = "password";

    public string? UserName { get; init; }

    public string? Password { get; init; }

    public string? RefreshToken { get; init; }

    public string? Scope { get; init; }

    public FormUrlEncodedContent ToFormUrlEncodedContent()
    {
        var fields = new Dictionary<string, string>
        {
            ["grant_type"] = GrantType
        };

        AddIfNotEmpty(fields, "userName", UserName);
        AddIfNotEmpty(fields, "password", Password);
        AddIfNotEmpty(fields, "refresh_token", RefreshToken);
        AddIfNotEmpty(fields, "scope", Scope);

        return new FormUrlEncodedContent(fields);
    }

    private static void AddIfNotEmpty(IDictionary<string, string> fields, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            fields[name] = value;
        }
    }
}
