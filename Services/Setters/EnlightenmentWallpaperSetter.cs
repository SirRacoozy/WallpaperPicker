namespace WallpaperPicker.Services.Setters;

class EnlightenmentWallpaperSetter : BaseWallpaperSetter
{
    public override bool Set(string filePath) =>
        Exec("enlightenment_remote", "-desktop-bg-set", "0", "0", "0", "0", filePath).Ok;
}
