namespace WallpaperPicker.Services.Setters;

class MateWallpaperSetter : BaseWallpaperSetter
{
    public override bool Set(string filePath) =>
        Exec("gsettings", "set", "org.mate.background", "picture-filename", filePath).Ok;
}
