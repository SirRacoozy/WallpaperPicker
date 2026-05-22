using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace WallpaperPicker.Views;

partial class FavoritesWindow : Window
{
    private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(30) };

    private readonly TextBlock _emptyHint;

    public FavoritesWindow()
    {
        InitializeComponent();

        Title = L.Get("favTitle");

        _emptyHint = new TextBlock
        {
            Text = L.Get("favEmpty"),
            Foreground = Brushes.Gray,
            FontSize = 16,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 40),
        };

        _ = LoadFavorites();
    }

    private async Task LoadFavorites()
    {
        ImageGrid.Children.Clear();

        var seeds = FavoritesManager.Seeds.ToList();
        if (seeds.Count == 0) { ImageGrid.Children.Add(_emptyHint); return; }

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
                Content = L.Get("setBtn"),
                Background = Brushes.DarkGreen,
                Foreground = Brushes.White,
                Cursor = new Cursor(StandardCursorType.Hand),
                Margin = new Thickness(0, 4, 4, 0),
                FontSize = 12,
            };

            var removeBtn = new Button
            {
                Content = L.Get("favRemove"),
                Background = Brushes.DarkRed,
                Foreground = Brushes.White,
                Cursor = new Cursor(StandardCursorType.Hand),
                Margin = new Thickness(0, 4, 0, 0),
                FontSize = 12,
            };

            var btnRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Children = { setBtn, removeBtn },
            };

            var card = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(5),
                Padding = new Thickness(6),
                Child = new StackPanel { Children = { img, btnRow } },
            };

            setBtn.Click += async (_, _) =>
            {
                setBtn.IsEnabled = false;
                removeBtn.IsEnabled = false;
                try
                {
                    var data = await _http.GetByteArrayAsync(highResUrl);
                    var filePath = Path.Combine(Path.GetTempPath(), $"wallpaper_{DateTimeOffset.Now.ToUnixTimeSeconds()}.jpg");
                    await File.WriteAllBytesAsync(filePath, data);
                    await Task.Run(() => WallpaperSetter.Set(filePath));
                }
                finally
                {
                    setBtn.IsEnabled = true;
                    removeBtn.IsEnabled = true;
                }
            };

            removeBtn.Click += (_, _) =>
            {
                FavoritesManager.Remove(seed);
                ImageGrid.Children.Remove(card);
                if (ImageGrid.Children.Count == 0)
                    ImageGrid.Children.Add(_emptyHint);
            };

            ImageGrid.Children.Add(card);
        }
        catch { }
    }
}
