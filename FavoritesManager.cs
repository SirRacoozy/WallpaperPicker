using System.Text.Json;

static class FavoritesManager
{
    private static readonly string _path = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WallpaperPicker", "favorites.json");

    private static List<string> _seeds = [];

    public static IReadOnlyList<string> Seeds => _seeds;

    public static void Load()
    {
        try
        {
            if (!File.Exists(_path)) return;
            _seeds = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(_path)) ?? [];
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
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        File.WriteAllText(_path, JsonSerializer.Serialize(_seeds));
    }
}
