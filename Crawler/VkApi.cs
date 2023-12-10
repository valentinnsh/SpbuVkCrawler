using System.Globalization;

namespace SpbuVkApiCrawler;

internal class VkApi
{
    private readonly string? _token;
    private readonly double _version;
    private readonly HttpClient _httpClient;

    public VkApi(string? token, double version)
    {
        _token = token;
        _version = version;
        _httpClient = new HttpClient();
    }

    public async Task<dynamic> NewsfeedSearchAsync(string? query, long startTime, long endTime)
    {
        var requestParams = new Dictionary<string, string?>
        {
            { "access_token", _token },
            { "v", _version.ToString(CultureInfo.InvariantCulture) },
            { "q", query },
            { "count", "200" },
            { "start_time", startTime.ToString() },
            { "end_time", endTime.ToString() }
        };

        return await SendRequestAsync("https://api.vk.com/method/newsfeed.search", requestParams);
    }

    public async Task<dynamic> WallGetAsync(string? domain, int offset)
    {
        var requestParams = new Dictionary<string, string?>
        {
            { "access_token", _token },
            { "v", _version.ToString(CultureInfo.InvariantCulture) },
            { "domain", domain },
            { "count", "100" },
            { "offset", offset.ToString() }
        };

        return await SendRequestAsync("https://api.vk.com/method/wall.get", requestParams);
    }

    private async Task<dynamic> SendRequestAsync(string url, Dictionary<string, string?> requestParams)
    {
        var requestUrl = url + "?" + string.Join("&", requestParams.Select(kvp => $"{kvp.Key}={kvp.Value}"));

        using var response = await _httpClient.GetAsync(requestUrl);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        return Newtonsoft.Json.JsonConvert.DeserializeObject(responseContent);
    }
}