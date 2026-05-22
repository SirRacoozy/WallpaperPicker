using WallpaperPicker.Services.Setters;

static class WallpaperSetter
{
    public static void Set(string filePath) =>
        SetterResolver.Resolve().Set(filePath);
}
