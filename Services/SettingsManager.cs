using System.Text.Json;
using WallpaperPicker;

static class SettingsManager
{
    private static readonly string Path = Constants.SettingsFilePath;

    public static int ImageCount { get; private set; } = 9;

    public static void Load()
    {
        try
        {
            if (!File.Exists(Path)) return;
            var data = JsonSerializer.Deserialize<SettingsData>(File.ReadAllText(Path));
            if (data != null) ImageCount = data.ImageCount;
        }
        catch
        {
            // ignored
        }
    }

    public static void SetImageCount(int count)
    {
        ImageCount = count;
        Save();
    }

    private static void Save()
    {
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path)!);
        File.WriteAllText(Path, JsonSerializer.Serialize(new SettingsData { ImageCount = ImageCount }));
    }

    private class SettingsData
    {
        public int ImageCount { get; set; } = 9;
    }
}
