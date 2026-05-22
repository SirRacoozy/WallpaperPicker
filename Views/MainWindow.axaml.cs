using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Serilog;

namespace WallpaperPicker.Views;

partial class MainWindow : Window
{
    private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(30) };

    private string? _selectedHighResUrl;
    private (string Key, string Arg) _status = ("ready", "");

    public MainWindow()
    {
        InitializeComponent();

        FavoritesManager.Load();
        SettingsManager.Load();

        var sysLang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        L.Current = Array.IndexOf(L.Codes, sysLang) >= 0 ? sysLang : "en";
        Log.Information("Language set to {Language}", L.Current);

        LangCombo.ItemsSource = L.Codes.Select(c => { var (f, n) = L.Languages[c]; return $"{f} {n}"; }).ToList();
        LangCombo.SelectedIndex = Array.IndexOf(L.Codes, L.Current);
        LangCombo.SelectionChanged += (_, _) =>
        {
            if (LangCombo.SelectedIndex >= 0)
            {
                L.Current = L.Codes[LangCombo.SelectedIndex];
                ApplyLanguage();
            }
        };

        var countOptions = new[] { 3, 6, 9, 12, 15, 18 };
        CountCombo.ItemsSource = countOptions;
        CountCombo.SelectedItem = countOptions.Contains(SettingsManager.ImageCount) ? SettingsManager.ImageCount : 9;
        CountCombo.SelectionChanged += async (_, _) =>
        {
            if (CountCombo.SelectedItem is int count)
            {
                Log.Information("Image count changed to {Count}", count);
                SettingsManager.SetImageCount(count);
                await LoadRandomImages();
            }
        };

        RefreshBtn.Click += async (_, _) => await LoadRandomImages();
        SetWallpaperBtn.Click += async (_, _) => await SetWallpaper();
        FavBtn.Click += (_, _) =>
        {
            Log.Information("Opening favorites window");
            var fw = new FavoritesWindow();
            fw.Show(this);
        };

        ApplyLanguage();
        _ = LoadRandomImages();
    }

    private void SetStatus(string key, string arg = "")
    {
        _status = (key, arg);
        StatusText.Text = L.Status(key, arg);
    }

    private void ApplyLanguage()
    {
        Title = L.Get("title");
        RefreshBtn.Content = L.Get("loadBtn");
        SetWallpaperBtn.Content = L.Get("setBtn");
        FavBtn.Content = L.Get("favBtn");
        StatusText.Text = L.Status(_status.Key, _status.Arg);
    }

    private async Task LoadRandomImages()
    {
        RefreshBtn.IsEnabled = false;
        SetWallpaperBtn.IsEnabled = false;
        _selectedHighResUrl = null;
        ImageGrid.Children.Clear();
        var count = SettingsManager.ImageCount;
        SetStatus("loading", count.ToString());
        Log.Information("Loading {Count} random images", count);

        await Task.WhenAll(Enumerable.Range(0, count).Select(async _ =>
        {
            var seed = Guid.NewGuid().ToString();
            var previewUrl = $"https://picsum.photos/seed/{seed}/300/200";
            var highResUrl = $"https://picsum.photos/seed/{seed}/2880/1800";

            try
            {
                var bytes = await _http.GetByteArrayAsync(previewUrl);
                using var ms = new MemoryStream(bytes);
                var bitmap = new Bitmap(ms);

                var border = new Border
                {
                    BorderBrush = Brushes.Transparent,
                    BorderThickness = new Thickness(4),
                    CornerRadius = new CornerRadius(5),
                    Child = new Avalonia.Controls.Image { Source = bitmap, Width = 300, Height = 200, Stretch = Stretch.UniformToFill },
                };

                var isFav = FavoritesManager.IsFavorite(seed);
                var starBtn = new Button
                {
                    Content = isFav ? "★" : "☆",
                    Foreground = isFav ? Brushes.Gold : Brushes.White,
                    Background = new SolidColorBrush(Color.FromArgb(140, 0, 0, 0)),
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(4, 2),
                    FontSize = 18,
                    Cursor = new Cursor(StandardCursorType.Hand),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, 4, 4, 0),
                    ZIndex = 1,
                };

                starBtn.Click += (_, e) =>
                {
                    e.Handled = true;
                    var nowFav = FavoritesManager.Toggle(seed);
                    var action = nowFav ? "added to" : "removed from";
                    Log.Information("Seed {Seed} {Action} favorites", seed, action);
                    starBtn.Content = nowFav ? "★" : "☆";
                    starBtn.Foreground = nowFav ? Brushes.Gold : Brushes.White;
                };

                var overlay = new Grid { Width = 300, Height = 200, Margin = new Thickness(5) };
                overlay.Children.Add(border);
                overlay.Children.Add(starBtn);

                var btn = new Button
                {
                    Content = overlay,
                    Padding = new Thickness(0),
                    Background = Brushes.Transparent,
                    Cursor = new Cursor(StandardCursorType.Hand),
                };

                btn.Click += (_, _) =>
                {
                    foreach (var child in ImageGrid.Children)
                        if (child is Button b && b.Content is Grid g && g.Children[0] is Border cBorder)
                            cBorder.BorderBrush = Brushes.Transparent;

                    border.BorderBrush = Brushes.DodgerBlue;
                    _selectedHighResUrl = highResUrl;
                    SetWallpaperBtn.IsEnabled = true;
                    SetStatus("selected");
                    Log.Information("Image selected: {Url}", highResUrl);
                };

                ImageGrid.Children.Add(btn);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to load preview image {Url}", previewUrl);
                SetStatus("errorLoad", ex.Message);
            }
        }));

        RefreshBtn.IsEnabled = true;
        SetStatus("loaded");
        Log.Information("Finished loading {Count} images", count);
    }

    private async Task SetWallpaper()
    {
        if (string.IsNullOrEmpty(_selectedHighResUrl)) return;

        SetWallpaperBtn.IsEnabled = false;
        RefreshBtn.IsEnabled = false;
        SetStatus("downloading");
        Log.Information("Downloading wallpaper from {Url}", _selectedHighResUrl);

        try
        {
            var bytes = await _http.GetByteArrayAsync(_selectedHighResUrl);
            var filePath = Path.Combine(Path.GetTempPath(), $"wallpaper_{DateTimeOffset.Now.ToUnixTimeSeconds()}.jpg");
            await File.WriteAllBytesAsync(filePath, bytes);

            SetStatus("setting");
            await Task.Run(() => WallpaperSetter.Set(filePath));
            SetStatus("done");
            Log.Information("Wallpaper set successfully from {Url}", _selectedHighResUrl);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to set wallpaper from {Url}", _selectedHighResUrl);
            SetStatus("errorSet", ex.Message);
        }
        finally
        {
            SetWallpaperBtn.IsEnabled = true;
            RefreshBtn.IsEnabled = true;
        }
    }
}
