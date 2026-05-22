using Avalonia;
using Serilog;
using WallpaperPicker;
using WallpaperPicker.App;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine(Constants.LogDirectory, "log-.txt"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14)
    .CreateLogger();

try
{
    Log.Information("Starting WallpaperPicker");
    AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .StartWithClassicDesktopLifetime(args);
    Log.Information("WallpaperPicker exited normally");
}
catch (Exception ex)
{
    Log.Fatal(ex, "WallpaperPicker terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
