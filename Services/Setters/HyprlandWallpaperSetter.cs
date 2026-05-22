namespace WallpaperPicker.Services.Setters;

class HyprlandWallpaperSetter : BaseWallpaperSetter
{
    public override bool Set(string filePath)
    {
        if (Exec("swww", "img", filePath).Ok)
            return true;
        return Exec("hyprctl", "hyprpaper", "wallpaper", $",{filePath}").Ok;
    }
}
