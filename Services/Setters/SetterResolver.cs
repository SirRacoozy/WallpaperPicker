namespace WallpaperPicker.Services.Setters;

static class SetterResolver
{
    public static IWallpaperSetter Resolve()
    {
        if (OperatingSystem.IsMacOS())
            return new MacOsWallpaperSetter();
        if (OperatingSystem.IsWindows())
            return new WindowsWallpaperSetter();

        return ResolveLinux();
    }

    private static IWallpaperSetter ResolveLinux()
    {
        var desktop = WallpaperHelper.Env("XDG_CURRENT_DESKTOP").ToLowerInvariant();
        var session = WallpaperHelper.Env("DESKTOP_SESSION").ToLowerInvariant();

        if (WallpaperHelper.Contains(desktop, session, "gnome", "unity", "budgie", "ubuntu"))
            return new GnomeWallpaperSetter();
        if (WallpaperHelper.Contains(desktop, session, "kde", "plasma"))
            return new KdeWallpaperSetter();
        if (WallpaperHelper.Contains(desktop, session, "xfce"))
            return new XfceWallpaperSetter();
        if (WallpaperHelper.Contains(desktop, session, "mate"))
            return new MateWallpaperSetter();
        if (WallpaperHelper.Contains(desktop, session, "cinnamon", "x-cinnamon"))
            return new CinnamonWallpaperSetter();
        if (WallpaperHelper.Contains(desktop, session, "lxqt"))
            return new LxqtWallpaperSetter();
        if (WallpaperHelper.Contains(desktop, session, "lxde"))
            return new LxdeWallpaperSetter();
        if (WallpaperHelper.Contains(desktop, session, "deepin", "dde"))
            return new DeepinWallpaperSetter();
        if (WallpaperHelper.Contains(desktop, session, "enlightenment"))
            return new EnlightenmentWallpaperSetter();
        if (WallpaperHelper.Contains(desktop, session, "sway") || !string.IsNullOrEmpty(WallpaperHelper.Env("SWAYSOCK")))
            return new SwayWallpaperSetter();
        if (WallpaperHelper.Contains(desktop, session, "hyprland") || !string.IsNullOrEmpty(WallpaperHelper.Env("HYPRLAND_INSTANCE_SIGNATURE")))
            return new HyprlandWallpaperSetter();
        if (WallpaperHelper.Contains(desktop, session, "i3", "openbox", "fluxbox", "icewm", "jwm"))
            return new WmFallbackWallpaperSetter();
        return new GenericFallbackWallpaperSetter();
    }
}
