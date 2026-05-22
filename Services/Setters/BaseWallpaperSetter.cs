namespace WallpaperPicker.Services.Setters;

abstract class BaseWallpaperSetter : IWallpaperSetter
{
    public abstract bool Set(string filePath);

    protected static bool Contains(string desktop, string session, params string[] keywords) =>
        WallpaperHelper.Contains(desktop, session, keywords);

    protected static string Env(string key) =>
        WallpaperHelper.Env(key);

    protected static (bool Ok, string? Output) Exec(string cmd, params string[] args) =>
        WallpaperHelper.Exec(cmd, args);
}
