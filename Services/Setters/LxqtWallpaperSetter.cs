namespace WallpaperPicker.Services.Setters;

class LxqtWallpaperSetter : BaseWallpaperSetter
{
    public override bool Set(string filePath) =>
        Exec("pcmanfm-qt", "--set-wallpaper", filePath).Ok;
}
