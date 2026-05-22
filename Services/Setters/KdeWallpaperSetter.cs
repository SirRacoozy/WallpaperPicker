namespace WallpaperPicker.Services.Setters;

class KdeWallpaperSetter : BaseWallpaperSetter
{
    public override bool Set(string filePath)
    {
        if (Exec("plasma-apply-wallpaperimage", filePath).Ok)
            return true;

        var script = $@"var all = desktops(); for (var i = 0; i < all.length; i++) {{ var d = all[i]; d.wallpaperPlugin = 'org.kde.image'; d.currentConfigGroup = ['Wallpaper', 'org.kde.image', 'General']; d.writeConfig('Image', 'file://{filePath}'); }}";
        return Exec("qdbus", "org.kde.plasmashell", "/PlasmaShell", "org.kde.PlasmaShell.evaluateScript", script).Ok;
    }
}
