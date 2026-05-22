namespace WallpaperPicker.Services.Setters;

class GnomeWallpaperSetter : BaseWallpaperSetter
{
    public override bool Set(string filePath)
    {
        var fileUri = $"file://{filePath}";
        var ok = Exec("gsettings", "set", "org.gnome.desktop.background", "picture-uri", fileUri).Ok;
        Exec("gsettings", "set", "org.gnome.desktop.background", "picture-uri-dark", fileUri);
        return ok;
    }
}
