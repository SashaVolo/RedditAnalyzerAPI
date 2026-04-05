
using System.Text.Json.Serialization;

namespace ReddirWebApi.Models;

// Главный класс запроса
    public class AnalyzerRequestDto
    {
        [JsonPropertyName("items")]
        public List<SubredditItemDto> Items { get; set; } = new();

        [JsonPropertyName("limit")]
        public int Limit { get; set; } = 25; // По умолчанию качаем 25, если клиент не указал
    }

    // Класс для каждого сабреддита внутри массива items
    public class SubredditItemDto
    {
        [JsonPropertyName("subreddit")]
        public string Subreddit { get; set; } = string.Empty;

        [JsonPropertyName("keywords")]
        public List<string> Keywords { get; set; } = new();
    }
