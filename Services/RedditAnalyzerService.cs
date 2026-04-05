using System.Text.Json;
using ReddirWebApi.Models;

namespace ReddirWebApi.Services;

public class RedditAnalyzerService : IRedditAnalyzerService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RedditAnalyzerService> _logger;

    public RedditAnalyzerService(IHttpClientFactory httpClientFactory, ILogger<RedditAnalyzerService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<Dictionary<string, List<PostResultDto>>> AnalyzeAsync(AnalyzerRequestDto request)
    {
        // Багатопоточність.
        // Всі HTTP-запити до сабреддітів стартують паралельно.
        var tasks = request.Items.Select(item => ProcessSubredditAsync(item, request.Limit));

        // Очікуємо завершення ВСІХ мережевих запитів
        var results = await Task.WhenAll(tasks);

        // Збираємо результати у словник. Відкидаємо ті, де сталася помилка (повернувся null)
        return results
            .Where(r => r.HasValue)
            .ToDictionary(r => r.Value.Key, r => r.Value.Value);
    }

    private async Task<KeyValuePair<string, List<PostResultDto>>?> ProcessSubredditAsync(SubredditItemDto item, int limit)
    {
        try
        {
            _logger.LogInformation("Починаю завантаження {Subreddit} (Ліміт: {Limit})", item.Subreddit, limit);

            // Отримуємо попередньо налаштований клієнт (з User-Agent) із фабрики
            var client = _httpClientFactory.CreateClient("RedditClient");

            // Формуємо URL. TrimEnd захищає від випадкових слешів в кінці: "r/nature/" -> "r/nature"
            var url = $"{item.Subreddit.TrimEnd('/')}/top.json?limit={limit}";

            var response = await client.GetAsync(url);
            
            // Викине виняток, якщо статус відповіді не 200 OK (наприклад, 404 Not Found)
            response.EnsureSuccessStatusCode(); 

            var jsonString = await response.Content.ReadAsStringAsync();

            // Десеріалізуємо JSON у наші C# об'єкти
            var redditData = JsonSerializer.Deserialize<RedditResponse>(jsonString);

            if (redditData?.Data?.Children == null)
            {
                _logger.LogWarning("Reddit не повернув постів для {Subreddit}", item.Subreddit);
                return new KeyValuePair<string, List<PostResultDto>>(item.Subreddit, new List<PostResultDto>());
            }

            var filteredPosts = new List<PostResultDto>();

            foreach (var child in redditData.Data.Children)
            {
                var post = child.Data;
                if (post == null) continue;

                // додатково: Фільтрація за заголовком ТА текстом поста (без урахування регістру)
                bool hasKeywordMatch = item.Keywords.Any(keyword =>
                    (post.Title != null && post.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (post.Selftext != null && post.Selftext.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                );

                if (hasKeywordMatch)
                {
                    filteredPosts.Add(new PostResultDto
                    {
                        Title = post.Title ?? "Без заголовка",
                        HasImage = CheckIfPostHasImage(post), // додатково: перевірка на наявність зображення
                        Url = $"https://www.reddit.com{post.Permalink}"
                    });
                }
            }

            _logger.LogInformation("Успішно оброблено {Subreddit}. Знайдено збігів: {Count}", item.Subreddit, filteredPosts.Count);

            return new KeyValuePair<string, List<PostResultDto>>(item.Subreddit, filteredPosts);
        }
        catch (HttpRequestException ex)
        {
            // додатково: Обробка мережевих помилок (невалідний URL, недоступність Reddit)
            _logger.LogError(ex, "Мережева помилка під час завантаження {Subreddit}", item.Subreddit);
            return null; 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непередбачена помилка під час обробки {Subreddit}", item.Subreddit);
            return null;
        }
    }

    // Допоміжний метод для перевірки, чи містить пост картинку
    private bool CheckIfPostHasImage(RedditPost post)
    {
        // Іноді Reddit сам прямо вказує, що це зображення у метаданих
        if (post.PostHint == "image") return true;

        // Іноді це просто пряме посилання на файл (перевіряємо розширення)
        if (!string.IsNullOrEmpty(post.Url))
        {
            var urlLower = post.Url.ToLower();
            if (urlLower.EndsWith(".jpg") || urlLower.EndsWith(".png") || urlLower.EndsWith(".gif") || urlLower.EndsWith(".jpeg"))
            {
                return true;
            }
        }

        return false;
    }
}