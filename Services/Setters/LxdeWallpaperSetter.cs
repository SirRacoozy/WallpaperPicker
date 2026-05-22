namespace WallpaperPicker.Services.Setters;

class LxdeWallpaperSetter : BaseWallpaperSetter
{
    public override bool Set(string filePath) =>
        Exec("pcmanfm", "--set-wallpaper", filePath).Ok;
}
