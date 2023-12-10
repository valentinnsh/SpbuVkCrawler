using System.Globalization;
using CsvHelper.Configuration;
using Newtonsoft.Json.Linq;

namespace SpbuVkApiCrawler.Models;

public class PostData
{
    public long Id { get; set; }
    public long OwnerId { get; set; }
    public long FromId { get; set; }
    public DateTime Date { get; set; }
    public string Text { get; set; }
    public int CommentsCount { get; set; }
    public int LikesCount { get; set; }
    public int RepostsCount { get; set; }
    public int ViewsCount { get; set; }

    public static PostData ToPostData(JObject post)
    {
        return new PostData
        {
            Id = (long)post["id"],
            OwnerId = (long)post["owner_id"],
            FromId = (long)post["from_id"],
            Date = DateTimeOffset.FromUnixTimeSeconds((long)post["date"]).DateTime,
            Text = post["text"].ToString() ?? string.Empty,
            CommentsCount = (int)post["comments"]["count"],
            LikesCount = (int)post["likes"]["count"],
            RepostsCount = (int)post["reposts"]["count"],
            ViewsCount = (int)post["views"]["count"]
        };
    }
}

// ReSharper disable once UnusedType.Global
public sealed class PostDataMap : ClassMap<PostData>
{
    public PostDataMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
    }
}