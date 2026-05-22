namespace WallpaperPicker.Services.Setters;

class XfceWallpaperSetter : BaseWallpaperSetter
{
    public override bool Set(string filePath)
    {
        var (ok, output) = Exec("xfconf-query", "-c", "xfce4-desktop", "-l");
        if (ok && output != null)
        {
            foreach (var line in output.Split('\n'))
            {
                var prop = line.Trim();
                if (prop.EndsWith("/last-image", StringComparison.Ordinal))
                    Exec("xfconf-query", "-c", "xfce4-desktop", "-p", prop, "-s", filePath);
            }
            return true;
        }

        return Exec("xfconf-query", "-c", "xfce4-desktop", "-p", "/backdrop/screen0/monitor0/workspace0/last-image", "-s", filePath).Ok;
    }
}
