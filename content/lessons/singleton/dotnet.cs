Parallel.For(0, 10, i => { _ = AppConfig.Instance.ApiUrl; }); // ۱۰ نخ هم‌زمان می‌خواهند نمونه را بگیرند

Console.WriteLine(AppConfig.Instance == AppConfig.Instance); // True
Console.WriteLine(AppConfig.CreatedCount);                    // 1

sealed class AppConfig
{
    public static int CreatedCount;

    // Lazy<T> ساخت را thread-safe و تنبل می‌کند؛ بدون قفل دستی و بدون خطای double-checked locking
    private static readonly Lazy<AppConfig> _instance = new(() => new AppConfig());

    public static AppConfig Instance => _instance.Value;

    private AppConfig()
    {
        Interlocked.Increment(ref CreatedCount);
        // خواندن appsettings و ... (گران)
    }

    public string ApiUrl { get; } = "https://api.example.com";
}

// در ASP.NET Core معمولاً کلاس دستی Singleton نمی‌نویسیم؛ DI همین را می‌دهد:
//   builder.Services.AddSingleton<IAppConfig, AppConfig>();
// این‌طوری سازنده public/تزریق‌پذیر و در تست قابل جایگزینی است.
