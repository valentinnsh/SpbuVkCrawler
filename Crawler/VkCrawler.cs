using CsvHelper;
using Newtonsoft.Json.Linq;
using SpbuVkApiCrawler.Models;

namespace SpbuVkApiCrawler;

internal class VkCrawler
{
    private static readonly Random Random = new ();
    private readonly VkApi _vkApi;
    private readonly long _startTime;
    private readonly long _endTime;
    private readonly int? _maxPostsCount;
    private readonly CsvWriter _csvWriter;
    private readonly List<PostData> _feedPosts;
    private readonly HashSet<string> _postIds;

    public VkCrawler(VkApi vkApi, DateTime startTime, DateTime endTime, CsvWriter csvWriter, int? maxPostsCount = null)
    {
        _vkApi = vkApi;
        _startTime = ((DateTimeOffset)startTime).ToUnixTimeSeconds();
        _endTime = ((DateTimeOffset)endTime).ToUnixTimeSeconds();
        _maxPostsCount = maxPostsCount;
        _csvWriter = csvWriter;
        _feedPosts = new List<PostData>();
        _postIds = new HashSet<string>();
    }

    public async Task GetByQueryAsync(string? query)
    {
        var lastRecordEndTime = _endTime;

        while (_maxPostsCount == null || _feedPosts.Count < _maxPostsCount)
        {
            var response = await GetQueryResponseAsync(query, lastRecordEndTime);
            var items = response["response"]["items"];
            if (items.Count == 0 || items[0]["date"] == lastRecordEndTime)
            {
                break;
            }

            lastRecordEndTime = items[items.Count - 1]["date"];
            Thread.Sleep((int)(Random.NextDouble() * 100));
            
            foreach (var post in items)
            {
                AddPost(post);
            }

            Console.WriteLine($"Total posts {_feedPosts.Count}, last record time {lastRecordEndTime}\n");
        }
    }

    public async Task GetByGroupAsync(string? domain)
    {
        var offset = 0;
        var lastRecordEndTime = _endTime;

        while (_maxPostsCount == null || _feedPosts.Count < _maxPostsCount)
        {
            var response = await GetGroupResponseAsync(domain, offset);
            var items = response["response"]["items"];

            if (items.Count == 0 || (lastRecordEndTime < _startTime || items[0]["date"] == lastRecordEndTime))
            {
                break;
            }

            lastRecordEndTime = items["date"];
            // Sleep for some random time to not get rate-limited by Vk XO
            Thread.Sleep((int)(Random.NextDouble() * 100));

            foreach (var post in items)
            {
                AddPost(post);
            }

            offset += items.Count;
            Console.WriteLine($"Total posts {_feedPosts.Count}, last record time {lastRecordEndTime}\n");
        }
    }

    private async Task<dynamic> GetQueryResponseAsync(string? query, long lastRecordEndTime)
    {
        return await _vkApi.NewsfeedSearchAsync(query, _startTime, lastRecordEndTime);
    }

    private async Task<dynamic> GetGroupResponseAsync(string? domain, int offset)
    {
        return await _vkApi.WallGetAsync(domain, offset);
    }

    private void AddPost(JObject post)
    {
        try
        {
            var postId = $"{post["id"]}_{post["owner_id"]}";

            if (_postIds.Contains(postId)) return;
            var postDate = post["date"].Value<int>();
            if (postDate <= _startTime || postDate >= _endTime) return;
            _postIds.Add(postId);
            var postData = PostData.ToPostData(post);
            _feedPosts.Add(postData);

            _csvWriter.WriteRecord(postData);
            _csvWriter.NextRecord();
        }
        catch(Exception e)
        {
            Console.WriteLine($"Encountered error {e}");
        }
    }
}