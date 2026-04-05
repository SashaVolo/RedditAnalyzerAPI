using System;
using System.Text.Json.Serialization;

namespace ReddirWebApi.Models
{
    // Этот класс описывает один отфильтрованный пост в итоговом ответе
    public class PostResultDto
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        // додатково: додаємо поле для зберігання URL зображення
        [JsonPropertyName("hasImage")]
        public bool HasImage { get; set; }
        public string? Url { get; set; }
    }
}