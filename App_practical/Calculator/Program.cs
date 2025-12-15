using OpenTelemetry.Metrics;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddOpenTelemetry() // Добавляем OpenTelemetry в систему через DI (Dependency Injection).
    .WithMetrics(meterProviderBuilder => // Настраиваем сбор метрик.
    {
        meterProviderBuilder.AddPrometheusExporter();
        // Экспортируем метрики в формате, поддерживаемом Prometheus.

        // Добавляем метрики, специфичные для ASP.NET Core приложений.
        meterProviderBuilder.AddMeter("Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel"); //Сбор метрик с хостинга и сервера Kestrel, которые используются для работы ASP.NET Core приложений.
        // Добавляем метрики для HTTP соединений, используемых в приложении.
        meterProviderBuilder.AddMeter("Microsoft.AspNetCore.Http.Connections"); // Сбор метрик, связанных с HTTP соединениями, таких как открытые или активные подключения.
        // Настраиваем метрику для измерения продолжительности обработки HTTP запросов.
        meterProviderBuilder.AddView("http.server.request.duration", // Счетчик, измеряющий длительность обработки HTTP запросов.
            new ExplicitBucketHistogramConfiguration //Конфигурация для распределения метрик по гистограммам.
            {
                Boundaries = // Указываем границы ведения подсчетов для различных временных диапазонов.
                    [
                    0, // Меньше 0 секунд.
                    0.005, // Меньше 5 миллисекунд.
                    0.01, // Меньше 10 миллисекунд.
                    0.025, // Меньше 25 миллисекунд.
                    0.05, // Меньше 50 миллисекунд.
                    0.075, // Меньше 75 миллисекунд.
                    0.1, // Меньше 100 миллисекунд.
                    0.25, // Меньше 250 миллисекунд.
                    0.5, // Меньше 500 миллисекунд.
                    0.75, // Меньше 750 миллисекунд.
                    1, // Меньше 1 секунды.
                    2.5, // Меньше 2.5 секунд.
                    5, // Меньше 5 секунд.
                    7.5, // Меньше 7.5 секунд.
                    10 // Меньше 10 секунд.
                    ]
            });
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Calculator}/{action=Index}/{id?}");

app.MapPrometheusScrapingEndpoint();

app.Run();