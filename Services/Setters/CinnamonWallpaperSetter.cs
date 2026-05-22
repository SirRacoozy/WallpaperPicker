namespace WallpaperPicker.Services.Setters;

class CinnamonWallpaperSetter : BaseWallpaperSetter
{
    public override bool Set(string filePath)
    {
        var fileUri = $"file://{filePath}";
        return Exec("gsettings", "set", "org.cinnamon.desktop.background", "picture-uri", fileUri).Ok;
    }
}
