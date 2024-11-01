using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class Auth
{
    private const string ClientId = "30ddfd9bed6940ddbc1855eb06dc2fac";
    private const string ClientSecret = "848f469455624eb1af77b96e73a4de86";
    private const string RedirectUri = "https://mysite.com/callback";
    private const string AuthUrl = "https://accounts.spotify.com/authorize";
    private const string TokenUrl = "https://accounts.spotify.com/api/token";

    public static string AccessToken { get; private set; }
    private static string _refreshToken;
    public static DateTime TokenExpirationTime { get; private set; }

    private const string GeniusToken = "nTDQn4bkgolLGcoug3o_4J53UKHGIMmEZsemzpvjm9UkfrtEps4efgekRdziDtlv";
    private const string GeniusApiUrl = "https://api.genius.com/search";
    public static async Task<string> GetAuthorizationCodeAsync()
    {
        string authorizationUri = $"{AuthUrl}?client_id={ClientId}&response_type=code&redirect_uri={Uri.EscapeDataString(RedirectUri)}&scope=user-read-playback-state";

        Console.WriteLine("Please log in to Spotify and authorize the app by visiting the following URL:");
        Console.WriteLine(authorizationUri);
        Console.WriteLine("\nAfter authorizing, you will be redirected to a URL. Paste that entire URL here:");

        string redirectedUrl = Console.ReadLine();
        
        if (Uri.TryCreate(redirectedUrl, UriKind.Absolute, out var uri))
        {
            var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
            string code = queryParams["code"];

            if (!string.IsNullOrEmpty(code))
            {
                return code;
            }

            Console.WriteLine("Error: Authorization code not found in the URL.");
        }
        else
        {
            Console.WriteLine("Error: Invalid URL. Make sure you copied the full URL after authorization.");
        }

        return null;
    }

    public static async Task InitializeTokensAsync(string authCode)
    {
        try
        {
            var tokenData = await GetAccessTokenAsync(authCode);
            AccessToken = tokenData.accessToken;
            _refreshToken = tokenData.refreshToken;
            TokenExpirationTime = DateTime.Now.AddSeconds(tokenData.expiresIn);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing tokens: {ex.Message}");
        }
    }
    private static async Task<(string accessToken, string refreshToken, int expiresIn)> GetAccessTokenAsync(string authorizationCode)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = CreateBasicAuthHeader();

        var requestContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", authorizationCode),
            new KeyValuePair<string, string>("redirect_uri", RedirectUri),
        });

        var response = await client.PostAsync(TokenUrl, requestContent);

        return await HandleTokenResponseAsync(response);
    }
    public static async Task RefreshAccessTokenAsync()
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = CreateBasicAuthHeader();

        var requestContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("refresh_token", _refreshToken),
        });

        var response = await client.PostAsync(TokenUrl, requestContent);
        Console.WriteLine($"Response Status: {response.StatusCode}");

        if (response.IsSuccessStatusCode)
        {
            var tokenData = await HandleTokenResponseAsync(response);
            AccessToken = tokenData.accessToken;
            TokenExpirationTime = DateTime.Now.AddSeconds(tokenData.expiresIn);
        }
        else
        {
            Console.WriteLine("Error refreshing access token: " + await response.Content.ReadAsStringAsync());
        }
    }
    public static async Task<string> GetLyricsAsync(string trackName, string artistName)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GeniusToken);

        var searchUrl = $"{GeniusApiUrl}?q={Uri.EscapeDataString($"{trackName} {artistName}")}";
        var response = await client.GetAsync(searchUrl);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("Error retrieving lyrics from Genius.");
            return null;
        }

        var jsonDoc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        var hits = jsonDoc.RootElement.GetProperty("response").GetProperty("hits");
        if (hits.GetArrayLength() > 0)
        {
            var songUrl = hits[0].GetProperty("result").GetProperty("url").GetString();
            Console.WriteLine($"Lyrics available at: {songUrl}");
            return songUrl;
        }

        Console.WriteLine("No lyrics found.");
        return null;
    }

    private static AuthenticationHeaderValue CreateBasicAuthHeader()
    {
        var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ClientId}:{ClientSecret}"));
        return new AuthenticationHeaderValue("Basic", authHeader);
    }

    private static async Task<(string accessToken, string refreshToken, int expiresIn)> HandleTokenResponseAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to retrieve token. Status Code: " + response.StatusCode);
        }

        var json = await response.Content.ReadAsStringAsync();
        var tokenData = JsonSerializer.Deserialize<JsonElement>(json);

        return (
            accessToken: tokenData.GetProperty("access_token").GetString(),
            refreshToken: tokenData.GetProperty("refresh_token").GetString(),
            expiresIn: tokenData.GetProperty("expires_in").GetInt32()
        );
    }
}
