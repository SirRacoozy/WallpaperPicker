namespace WallpaperPicker.Services.Setters;

class DeepinWallpaperSetter : BaseWallpaperSetter
{
    public override bool Set(string filePath)
    {
        var fileUri = $"file://{filePath}";
        return Exec("gsettings", "set", "com.deepin.wrap.gnome.desktop.background", "picture-uri", fileUri).Ok;
    }
}
