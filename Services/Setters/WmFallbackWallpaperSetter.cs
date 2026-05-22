namespace WallpaperPicker.Services.Setters;

class WmFallbackWallpaperSetter : BaseWallpaperSetter
{
    public override bool Set(string filePath)
    {
        if (Exec("feh", "--bg-fill", filePath).Ok)
            return true;
        return Exec("nitrogen", "--set-zoom-fill", filePath).Ok;
    }
}
