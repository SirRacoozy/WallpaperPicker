using System.Text.Json;
using WallpaperPicker;

static class FavoritesManager
{
    private static readonly string Path = Constants.FavoritesFilePath;

    private static List<string> _seeds = [];

    public static IReadOnlyList<string> Seeds => _seeds;

    public static void Load()
    {
        try
        {
            if (!File.Exists(Path)) return;
            _seeds = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(Path)) ?? [];
        }
        catch { _seeds = []; }
    }

    public static bool IsFavorite(string seed) => _seeds.Contains(seed);

    public static bool Toggle(string seed)
    {
        if (_seeds.Contains(seed)) { _seeds.Remove(seed); Save(); return false; }
        else                       { _seeds.Add(seed);    Save(); return true; }
    }

    public static void Remove(string seed)
    {
        _seeds.Remove(seed);
        Save();
    }

    private static void Save()
    {
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path)!);
        File.WriteAllText(Path, JsonSerializer.Serialize(_seeds));
    }
}
