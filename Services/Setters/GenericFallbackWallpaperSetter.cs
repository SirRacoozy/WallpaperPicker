namespace WallpaperPicker.Services.Setters;

class GenericFallbackWallpaperSetter : BaseWallpaperSetter
{
    public override bool Set(string filePath)
    {
        if (Exec("feh", "--bg-fill", filePath).Ok) return true;
        if (Exec("nitrogen", "--set-zoom-fill", filePath).Ok) return true;

        var fileUri = $"file://{filePath}";
        if (Exec("gsettings", "set", "org.gnome.desktop.background", "picture-uri", fileUri).Ok) return true;
        return Exec("plasma-apply-wallpaperimage", filePath).Ok;
    }
}
