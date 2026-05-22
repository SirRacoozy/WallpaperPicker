using Serilog;

namespace WallpaperPicker.Services.Setters;

static class SetterResolver
{
    public static IWallpaperSetter Resolve()
    {
        if (OperatingSystem.IsMacOS())
        {
            Log.Debug("Detected OS: macOS");
            return LogSetter(new MacOsWallpaperSetter());
        }
        if (OperatingSystem.IsWindows())
        {
            Log.Debug("Detected OS: Windows");
            return LogSetter(new WindowsWallpaperSetter());
        }

        return ResolveLinux();
    }

    private static IWallpaperSetter ResolveLinux()
    {
        var desktop = WallpaperHelper.Env("XDG_CURRENT_DESKTOP").ToLowerInvariant();
        var session = WallpaperHelper.Env("DESKTOP_SESSION").ToLowerInvariant();
        Log.Debug("Detected Linux DE: Desktop={Desktop}, Session={Session}", desktop, session);

        IWallpaperSetter setter;

        if (WallpaperHelper.Contains(desktop, session, "gnome", "unity", "budgie", "ubuntu"))
            setter = new GnomeWallpaperSetter();
        else if (WallpaperHelper.Contains(desktop, session, "kde", "plasma"))
            setter = new KdeWallpaperSetter();
        else if (WallpaperHelper.Contains(desktop, session, "xfce"))
            setter = new XfceWallpaperSetter();
        else if (WallpaperHelper.Contains(desktop, session, "mate"))
            setter = new MateWallpaperSetter();
        else if (WallpaperHelper.Contains(desktop, session, "cinnamon", "x-cinnamon"))
            setter = new CinnamonWallpaperSetter();
        else if (WallpaperHelper.Contains(desktop, session, "lxqt"))
            setter = new LxqtWallpaperSetter();
        else if (WallpaperHelper.Contains(desktop, session, "lxde"))
            setter = new LxdeWallpaperSetter();
        else if (WallpaperHelper.Contains(desktop, session, "deepin", "dde"))
            setter = new DeepinWallpaperSetter();
        else if (WallpaperHelper.Contains(desktop, session, "enlightenment"))
            setter = new EnlightenmentWallpaperSetter();
        else if (WallpaperHelper.Contains(desktop, session, "sway") || !string.IsNullOrEmpty(WallpaperHelper.Env("SWAYSOCK")))
            setter = new SwayWallpaperSetter();
        else if (WallpaperHelper.Contains(desktop, session, "hyprland") || !string.IsNullOrEmpty(WallpaperHelper.Env("HYPRLAND_INSTANCE_SIGNATURE")))
            setter = new HyprlandWallpaperSetter();
        else if (WallpaperHelper.Contains(desktop, session, "i3", "openbox", "fluxbox", "icewm", "jwm"))
            setter = new WmFallbackWallpaperSetter();
        else
            setter = new GenericFallbackWallpaperSetter();

        return LogSetter(setter);
    }

    private static IWallpaperSetter LogSetter(IWallpaperSetter setter)
    {
        Log.Debug("Using wallpaper setter: {Setter}", setter.GetType().Name);
        return setter;
    }
}
