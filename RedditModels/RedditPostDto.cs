using System.Text.Json.Serialization;

namespace ReddirWebApi.Models
{
    public class RedditResponse
    {
        [JsonPropertyName("data")]
        public RedditData? Data { get; set; }
    }

    public class RedditData
    {
        [JsonPropertyName("children")]
        public RedditPostWrapper[]? Children { get; set; } 
    }

    public class RedditPostWrapper
    {
        [JsonPropertyName("data")]
        public RedditPost? Data { get; set; } 
    }

    public class RedditPost
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; } 

        [JsonPropertyName("selftext")]
        public string? Selftext { get; set; } 

        [JsonPropertyName("url")]
        public string? Url { get; set; } 

        [JsonPropertyName("post_hint")]
        public string? PostHint { get; set; } 
        
        [JsonPropertyName("permalink")]
        public string? Permalink { get; set; }
    }
}