using ReddirWebApi.Services;
using Serilog;

// налаштовуємо Serilog перед створенням билдера, щоб схопити помилки старту
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/out.log", rollingInterval: RollingInterval.Day) // Пишемо логи в out.log
    .CreateLogger();

try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    // Говоримо ASP.NET використовувати Serilog замість стандартного логгера
    builder.Host.UseSerilog();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    //Налаштовуємо HttpClient спеціально для Reddit
    builder.Services.AddHttpClient("RedditClient", client =>
    {
        client.BaseAddress = builder.Configuration.GetSection("RedditApi:BaseUrl").Get<Uri>();
        // Reddit вимагає унікальний User-Agent, інакше поверне помилку 429
        client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "RedditAnalyzerApp/1.0");
    });

    // Регистрируем сервис 
    builder.Services.AddScoped<IRedditAnalyzerService, RedditAnalyzerService>();

    var app = builder.Build();


    app.UseSwagger();
    app.UseSwaggerUI();


    // Добавляем middleware для автоматического логирования HTTP-запросов
    app.UseSerilogRequestLogging();

    // додатково: (UI)
    app.UseDefaultFiles(); // Шукає index.html за замовчуванням
    app.UseStaticFiles();  // Дозволяє віддавати файли з папки wwwroot

    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}