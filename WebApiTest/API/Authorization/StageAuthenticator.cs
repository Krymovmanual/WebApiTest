using WebApiTest.API.Configuration;
using Newtonsoft.Json;

namespace WebApiTest.API.Authorization;

public static class StageAuthenticator
{
    private static readonly HttpClient TokenClient = new()
    {
        BaseAddress = new Uri(ApiClientBase.StageBaseUrl)
    };

    private static readonly SemaphoreSlim TokenSemaphore = new(1, 1);
    private static string? accessToken;
    private static DateTime tokenCreatedAtUtc;

    public static async Task<string> GetAccessTokenAsync()
    {
        if (HasValidToken())
        {
            return accessToken!;
        }

        await TokenSemaphore.WaitAsync();
        try
        {
            if (HasValidToken())
            {
                return accessToken!;
            }

            var credentials = StageCredentialsProvider.GetCredentials();
            using var requestBody = new StageTokenRequest
            {
                UserName = credentials.UserName,
                Password = credentials.Password
            }.ToFormUrlEncodedContent();

            using var response = await TokenClient.PostAsync("/api/token", requestBody);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Failed to get stage access token. Status: {(int)response.StatusCode} {response.StatusCode}. Body: {content}");
            }

            var tokenResponse = JsonConvert.DeserializeObject<StageTokenResponse>(content);
            if (string.IsNullOrWhiteSpace(tokenResponse?.AccessToken))
            {
                var errorDetails = string.IsNullOrWhiteSpace(tokenResponse?.Error)
                    ? content
                    : $"{tokenResponse.Error}: {tokenResponse.ErrorDescription}";

                throw new InvalidOperationException($"Stage token response did not contain access_token. Body: {errorDetails}");
            }

            accessToken = tokenResponse.AccessToken;
            tokenCreatedAtUtc = DateTime.UtcNow;

            return accessToken;
        }
        finally
        {
            TokenSemaphore.Release();
        }
    }

    private static bool HasValidToken()
    {
        return !string.IsNullOrWhiteSpace(accessToken) &&
               DateTime.UtcNow - tokenCreatedAtUtc < TimeSpan.FromHours(1);
    }
}
