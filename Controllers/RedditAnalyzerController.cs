using Microsoft.AspNetCore.Mvc;
using ReddirWebApi.Models;
using ReddirWebApi.Services;

namespace ReddirWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RedditAnalyzerController : ControllerBase
    {
        private readonly IRedditAnalyzerService _analyzerService;
        private readonly ILogger<RedditAnalyzerController> _logger;

        public RedditAnalyzerController(IRedditAnalyzerService analyzerService, ILogger<RedditAnalyzerController> logger)
        {
            _analyzerService = analyzerService;
            _logger = logger;
        }

        // POST-метод для обробки запитів за адресою: api/redditanalyzer
        [HttpPost]
        public async Task<IActionResult> Analyze([FromBody] AnalyzerRequestDto request)
        {
            _logger.LogInformation("Отримано HTTP-запит на аналіз {Count} сабреддітів.", request.Items?.Count ?? 0);

            // перевіряємо, чи надіслав клієнт хоча б один елемент для аналізу
            if (request.Items == null || request.Items.Count == 0)
            {
                _logger.LogWarning("Клієнт надіслав порожній список сабреддітів.");
                return BadRequest("Список сабреддітів (items) не може бути порожнім.");
            }

            try
            {
                // Делегуємо всю важку роботу сервісу
                var result = await _analyzerService.AnalyzeAsync(request);
                
                // додатково: Повернення результату у вигляді файлу
                // для JSON (щоб файл був читабельним)
                var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                var jsonBytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(result, options);

                // Повертаємо файл reddit_analysis.json
                return File(jsonBytes, "application/json", "reddit_analysis.json");
            }
            catch (Exception ex)
            {
                //гарантує, що API не "впаде" критично
                _logger.LogError(ex, "Глобальна помилка під час обробки запиту в контролері.");
                return StatusCode(500, "Внутрішня помилка сервера. Перевірте out.log для деталей.");
            }
        }
    }
}