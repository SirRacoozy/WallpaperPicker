namespace WallpaperPicker.Services.Setters;

class SwayWallpaperSetter : BaseWallpaperSetter
{
    public override bool Set(string filePath) =>
        Exec("swaymsg", "output", "*", "bg", filePath, "fill").Ok;
}
