using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Serilog;
using WallpaperPicker.Views;

namespace WallpaperPicker.App;

public class App : Application
{
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Log.Information("Avalonia framework initialized");
            desktop.MainWindow = new MainWindow();
        }
        base.OnFrameworkInitializationCompleted();
    }
}
