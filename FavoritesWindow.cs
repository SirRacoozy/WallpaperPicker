using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;

class FavoritesWindow : Window
{
    private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(30) };

    private readonly WrapPanel _grid;
    private readonly TextBlock _emptyHint;

    public FavoritesWindow()
    {
        Title  = L.Get("favTitle");
        Width  = 980;
        Height = 660;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Brushes.Black;

        _emptyHint = new TextBlock
        {
            Text              = L.Get("favEmpty"),
            Foreground        = Brushes.Gray,
            FontSize          = 16,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment   = VerticalAlignment.Center,
            Margin            = new Thickness(0, 40),
        };

        _grid = new WrapPanel
        {
            Orientation           = Orientation.Horizontal,
            HorizontalAlignment   = HorizontalAlignment.Center,
            Margin                = new Thickness(10),
        };

        var scroll = new ScrollViewer { Content = _grid };
        Content = scroll;

        _ = LoadFavorites();
    }

    private async Task LoadFavorites()
    {
        _grid.Children.Clear();

        var seeds = FavoritesManager.Seeds.ToList();
        if (seeds.Count == 0) { _grid.Children.Add(_emptyHint); return; }

        await Task.WhenAll(seeds.Select(seed => AddCard(seed)));
    }

    private async Task AddCard(string seed)
    {
        var previewUrl = $"https://picsum.photos/seed/{seed}/300/200";
        var highResUrl = $"https://picsum.photos/seed/{seed}/2880/1800";

        try
        {
            var bytes = await _http.GetByteArrayAsync(previewUrl);
            using var ms = new MemoryStream(bytes);
            var bitmap = new Bitmap(ms);

            var img = new Avalonia.Controls.Image { Source = bitmap, Width = 300, Height = 200, Stretch = Stretch.UniformToFill };

            var setBtn = new Button
            {
                Content    = L.Get("setBtn"),
                Background = Brushes.DarkGreen,
                Foreground = Brushes.White,
                Cursor     = new Cursor(StandardCursorType.Hand),
                Margin     = new Thickness(0, 4, 4, 0),
                FontSize   = 12,
            };

            var removeBtn = new Button
            {
                Content    = L.Get("favRemove"),
                Background = Brushes.DarkRed,
                Foreground = Brushes.White,
                Cursor     = new Cursor(StandardCursorType.Hand),
                Margin     = new Thickness(0, 4, 0, 0),
                FontSize   = 12,
            };

            var btnRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Children = { setBtn, removeBtn },
            };

            var card = new Border
            {
                Background      = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                CornerRadius    = new CornerRadius(6),
                Margin          = new Thickness(5),
                Padding         = new Thickness(6),
                Child           = new StackPanel { Children = { img, btnRow } },
            };

            setBtn.Click += async (_, _) =>
            {
                setBtn.IsEnabled    = false;
                removeBtn.IsEnabled = false;
                try
                {
                    var data     = await _http.GetByteArrayAsync(highResUrl);
                    var filePath = Path.Combine(Path.GetTempPath(), $"wallpaper_{DateTimeOffset.Now.ToUnixTimeSeconds()}.jpg");
                    await File.WriteAllBytesAsync(filePath, data);
                    await Task.Run(() => WallpaperSetter.Set(filePath));
                }
                finally
                {
                    setBtn.IsEnabled    = true;
                    removeBtn.IsEnabled = true;
                }
            };

            removeBtn.Click += (_, _) =>
            {
                FavoritesManager.Remove(seed);
                _grid.Children.Remove(card);
                if (_grid.Children.Count == 0)
                    _grid.Children.Add(_emptyHint);
            };

            _grid.Children.Add(card);
        }
        catch { /* skip failed previews silently */ }
    }
}
