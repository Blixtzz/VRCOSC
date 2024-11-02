using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.XPath;
using SocketCshart;

class StupidSpotifyWrapper
{
    public static bool isSong = false;
    private const string currentlyPlayingUrl = "https://api.spotify.com/v1/me/player/currently-playing";
    public static async Task<string> GetCurrentlyPlayingTrack()
    {
        if (DateTime.Now >= Auth.TokenExpirationTime)
        {
            Console.WriteLine("Access token expired. Refreshing...");
            await Auth.RefreshAccessTokenAsync();
        }

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Auth.AccessToken);

        var response = await client.GetAsync(currentlyPlayingUrl);
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(json);

            if (jsonDocument.RootElement.TryGetProperty("item", out var item) &&
                item.TryGetProperty("name", out var trackName))
            {
                var artistNames = string.Join(", ", item.GetProperty("artists")
                    .EnumerateArray()
                    .Select(artist => artist.GetProperty("name").GetString()));

                var durationMs = item.GetProperty("duration_ms").GetInt32();
                var progressMs = jsonDocument.RootElement.GetProperty("progress_ms").GetInt32();

                var duration = TimeSpan.FromMilliseconds(durationMs);
                var progress = TimeSpan.FromMilliseconds(progressMs);
                var track = trackName.GetString();
                if (track.Contains("("))
                {
                    track = trackName.GetString().Split("(")[0];                
                    string linet = $"{track}by {artistNames}" + "{/v}" + $"{progress.Minutes}:{progress.Seconds:D2} / {duration.Minutes}:{duration.Seconds:D2}";
                    return linet;
                }
                string line = $"{track} by {artistNames}" + "{/v}" + $"{progress.Minutes}:{progress.Seconds:D2} / {duration.Minutes}:{duration.Seconds:D2}";
                isSong = Song(line);
                return line;
            }
            else
            {
                Console.WriteLine("No song is currently playing.");
            }
        }
        else
        {
            Console.WriteLine("Error retrieving currently playing song.");
        }
        return "null";
    }
    private static string holder = "";
    private static bool Song(string input)
    {
        if (holder == input)
        {
            return true;
        }
        holder = input;
        return false; 
    }
}