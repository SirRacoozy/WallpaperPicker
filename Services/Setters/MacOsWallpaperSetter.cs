namespace WallpaperPicker.Services.Setters;

class MacOsWallpaperSetter : BaseWallpaperSetter
{
    public override bool Set(string filePath)
    {
        var script = $@"tell application ""System Events""
            set allDesktops to a reference to every desktop
            repeat with d in allDesktops
                set picture of d to ""{filePath}""
            end repeat
        end tell";
        return Exec("osascript", "-e", script).Ok;
    }
}
