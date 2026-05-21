using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Themes.Fluent;

AppBuilder.Configure<WallpaperApp>()
    .UsePlatformDetect()
    .StartWithClassicDesktopLifetime(args);

class WallpaperApp : Application
{
    public override void Initialize() => Styles.Add(new FluentTheme());

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow();
        base.OnFrameworkInitializationCompleted();
    }
}

class MainWindow : Window
{
    private static readonly string[] LangCodes = ["en", "de", "fr", "es", "it"];

    private readonly WrapPanel _imageGrid;
    private readonly Button _setWallpaperBtn;
    private readonly Button _refreshBtn;
    private readonly TextBlock _statusText;
    private readonly ComboBox _langCombo;
    private string? _selectedHighResUrl;
    private (string Key, string Arg) _status = ("ready", "");

    public MainWindow()
    {
        var sysLang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        L.Current = Array.IndexOf(LangCodes, sysLang) >= 0 ? sysLang : "en";

        Width = 1000;
        Height = 700;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = Brushes.Black;

        _refreshBtn = new Button { Margin = new Thickness(5), Background = Brushes.DarkSlateBlue, Foreground = Brushes.White, VerticalAlignment = VerticalAlignment.Center };
        _setWallpaperBtn = new Button { Margin = new Thickness(5), Background = Brushes.DarkGreen, Foreground = Brushes.White, IsEnabled = false, VerticalAlignment = VerticalAlignment.Center };
        _statusText = new TextBlock { Foreground = Brushes.LightGray, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(15, 0) };

        var langItems = new List<string>();
        foreach (var code in LangCodes)
        {
            var (flag, name) = L.Languages[code];
            langItems.Add($"{flag} {name}");
        }
        _langCombo = new ComboBox
        {
            ItemsSource = langItems,
            SelectedIndex = Array.IndexOf(LangCodes, L.Current),
            Margin = new Thickness(5),
            VerticalAlignment = VerticalAlignment.Center,
        };
        _langCombo.SelectionChanged += (_, _) =>
        {
            if (_langCombo.SelectedIndex >= 0)
            {
                L.Current = LangCodes[_langCombo.SelectedIndex];
                ApplyLanguage();
            }
        };

        var topBar = new DockPanel { Margin = new Thickness(10), Background = Brushes.Black };
        DockPanel.SetDock(_refreshBtn, Dock.Left);
        topBar.Children.Add(_refreshBtn);
        DockPanel.SetDock(_setWallpaperBtn, Dock.Left);
        topBar.Children.Add(_setWallpaperBtn);
        DockPanel.SetDock(_langCombo, Dock.Right);
        topBar.Children.Add(_langCombo);
        topBar.Children.Add(_statusText);

        _imageGrid = new WrapPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(10) };

        var mainPanel = new DockPanel();
        DockPanel.SetDock(topBar, Dock.Top);
        mainPanel.Children.Add(topBar);
        mainPanel.Children.Add(new ScrollViewer { Content = _imageGrid });
        Content = mainPanel;

        _refreshBtn.Click += async (_, _) => await LoadRandomImages();
        _setWallpaperBtn.Click += async (_, _) => await SetWallpaper();

        ApplyLanguage();
        _ = LoadRandomImages();
    }

    private void SetStatus(string key, string arg = "")
    {
        _status = (key, arg);
        _statusText.Text = L.Status(key, arg);
    }

    private void ApplyLanguage()
    {
        Title = L.Get("title");
        _refreshBtn.Content = L.Get("loadBtn");
        _setWallpaperBtn.Content = L.Get("setBtn");
        _statusText.Text = L.Status(_status.Key, _status.Arg);
    }

    private async Task LoadRandomImages()
    {
        _refreshBtn.IsEnabled = false;
        _setWallpaperBtn.IsEnabled = false;
        _selectedHighResUrl = null;
        _imageGrid.Children.Clear();
        SetStatus("loading");

        using var client = new HttpClient();

        for (int i = 0; i < 9; i++)
        {
            var seed = Guid.NewGuid().ToString();
            var previewUrl = $"https://picsum.photos/seed/{seed}/300/200";
            var highResUrl = $"https://picsum.photos/seed/{seed}/2880/1800";

            try
            {
                var bytes = await client.GetByteArrayAsync(previewUrl);
                using var ms = new MemoryStream(bytes);
                var bitmap = new Bitmap(ms);

                var border = new Border
                {
                    BorderBrush = Brushes.Transparent,
                    BorderThickness = new Thickness(4),
                    Margin = new Thickness(5),
                    CornerRadius = new CornerRadius(5),
                    Child = new Avalonia.Controls.Image { Source = bitmap, Width = 300, Height = 200, Stretch = Stretch.UniformToFill }
                };

                var btn = new Button
                {
                    Content = border,
                    Padding = new Thickness(0),
                    Background = Brushes.Transparent,
                    Cursor = new Cursor(StandardCursorType.Hand)
                };

                btn.Click += (_, _) =>
                {
                    foreach (var child in _imageGrid.Children)
                        if (child is Button b && b.Content is Border cBorder)
                            cBorder.BorderBrush = Brushes.Transparent;

                    border.BorderBrush = Brushes.DodgerBlue;
                    _selectedHighResUrl = highResUrl;
                    _setWallpaperBtn.IsEnabled = true;
                    SetStatus("selected");
                };

                _imageGrid.Children.Add(btn);
            }
            catch (Exception ex)
            {
                SetStatus("errorLoad", ex.Message);
            }
        }

        _refreshBtn.IsEnabled = true;
        SetStatus("loaded");
    }

    private async Task SetWallpaper()
    {
        if (string.IsNullOrEmpty(_selectedHighResUrl)) return;

        _setWallpaperBtn.IsEnabled = false;
        _refreshBtn.IsEnabled = false;
        SetStatus("downloading");

        try
        {
            using var client = new HttpClient();
            var bytes = await client.GetByteArrayAsync(_selectedHighResUrl);

            var filePath = Path.Combine(Path.GetTempPath(), $"wallpaper_{DateTimeOffset.Now.ToUnixTimeSeconds()}.jpg");
            await File.WriteAllBytesAsync(filePath, bytes);

            SetStatus("setting");
            await Task.Run(() => WallpaperSetter.Set(filePath));
            SetStatus("done");
        }
        catch (Exception ex)
        {
            SetStatus("errorSet", ex.Message);
        }
        finally
        {
            _setWallpaperBtn.IsEnabled = true;
            _refreshBtn.IsEnabled = true;
        }
    }
}

static class L
{
    public static string Current = "en";

    public static readonly Dictionary<string, (string Flag, string Name)> Languages = new()
    {
        ["en"] = ("🇬🇧", "English"),
        ["de"] = ("🇩🇪", "Deutsch"),
        ["fr"] = ("🇫🇷", "Français"),
        ["es"] = ("🇪🇸", "Español"),
        ["it"] = ("🇮🇹", "Italiano"),
    };

    private static readonly Dictionary<string, Dictionary<string, string>> _t = new()
    {
        ["en"] = new()
        {
            ["title"]       = "Wallpaper Picker",
            ["loadBtn"]     = "Load new images",
            ["setBtn"]      = "Set as wallpaper",
            ["ready"]       = "Click 'Load new images' to get started.",
            ["loading"]     = "Loading 9 images…",
            ["loaded"]      = "Images loaded. Pick one.",
            ["selected"]    = "Image selected. Ready to set.",
            ["downloading"] = "Downloading high-res image (2880×1800)…",
            ["setting"]     = "Setting wallpaper…",
            ["done"]        = "Done! Wallpaper updated.",
            ["errorLoad"]   = "Error loading: {0}",
            ["errorSet"]    = "Error: {0}",
        },
        ["de"] = new()
        {
            ["title"]       = "Hintergrundbild-Picker",
            ["loadBtn"]     = "Neue Bilder laden",
            ["setBtn"]      = "Als Hintergrund setzen",
            ["ready"]       = "Klicke auf 'Neue Bilder laden', um zu starten.",
            ["loading"]     = "Lade 9 neue Bilder herunter…",
            ["loaded"]      = "Bilder geladen. Wähle eins aus.",
            ["selected"]    = "Bild ausgewählt. Bereit zum Setzen.",
            ["downloading"] = "Lade High-Res Bild (2880×1800) herunter…",
            ["setting"]     = "Setze Hintergrund…",
            ["done"]        = "Erfolgreich! Hintergrund wurde aktualisiert.",
            ["errorLoad"]   = "Fehler beim Laden: {0}",
            ["errorSet"]    = "Fehler: {0}",
        },
        ["fr"] = new()
        {
            ["title"]       = "Sélecteur de fond d'écran",
            ["loadBtn"]     = "Charger de nouvelles images",
            ["setBtn"]      = "Définir comme fond d'écran",
            ["ready"]       = "Cliquez sur 'Charger de nouvelles images' pour commencer.",
            ["loading"]     = "Chargement de 9 images…",
            ["loaded"]      = "Images chargées. Choisissez-en une.",
            ["selected"]    = "Image sélectionnée. Prête à appliquer.",
            ["downloading"] = "Téléchargement haute résolution (2880×1800)…",
            ["setting"]     = "Application du fond d'écran…",
            ["done"]        = "Terminé ! Fond d'écran mis à jour.",
            ["errorLoad"]   = "Erreur de chargement : {0}",
            ["errorSet"]    = "Erreur : {0}",
        },
        ["es"] = new()
        {
            ["title"]       = "Selector de fondo de pantalla",
            ["loadBtn"]     = "Cargar nuevas imágenes",
            ["setBtn"]      = "Establecer como fondo",
            ["ready"]       = "Haz clic en 'Cargar nuevas imágenes' para comenzar.",
            ["loading"]     = "Cargando 9 imágenes…",
            ["loaded"]      = "Imágenes cargadas. Elige una.",
            ["selected"]    = "Imagen seleccionada. Lista para aplicar.",
            ["downloading"] = "Descargando imagen en alta resolución (2880×1800)…",
            ["setting"]     = "Aplicando fondo de pantalla…",
            ["done"]        = "¡Listo! Fondo de pantalla actualizado.",
            ["errorLoad"]   = "Error al cargar: {0}",
            ["errorSet"]    = "Error: {0}",
        },
        ["it"] = new()
        {
            ["title"]       = "Selettore sfondi",
            ["loadBtn"]     = "Carica nuove immagini",
            ["setBtn"]      = "Imposta come sfondo",
            ["ready"]       = "Clicca su 'Carica nuove immagini' per iniziare.",
            ["loading"]     = "Caricamento di 9 immagini…",
            ["loaded"]      = "Immagini caricate. Scegline una.",
            ["selected"]    = "Immagine selezionata. Pronta per l'impostazione.",
            ["downloading"] = "Download alta risoluzione (2880×1800)…",
            ["setting"]     = "Impostazione dello sfondo…",
            ["done"]        = "Fatto! Sfondo aggiornato.",
            ["errorLoad"]   = "Errore durante il caricamento: {0}",
            ["errorSet"]    = "Errore: {0}",
        },
    };

    public static string Get(string key) =>
        _t.TryGetValue(Current, out var d) && d.TryGetValue(key, out var v) ? v : key;

    public static string Status(string key, string arg = "") =>
        string.Format(Get(key), arg);
}

static class WallpaperSetter
{
    public static void Set(string filePath)
    {
        if (OperatingSystem.IsMacOS())
            SetMacOS(filePath);
        else if (OperatingSystem.IsWindows())
            SetWindows(filePath);
        else
            SetLinux(filePath);
    }

    private static void SetMacOS(string filePath)
    {
        var script = $@"tell application ""System Events""
            set allDesktops to a reference to every desktop
            repeat with d in allDesktops
                set picture of d to ""{filePath}""
            end repeat
        end tell";
        Run("osascript", "-e", script);
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

    private static void SetWindows(string filePath)
    {
        SystemParametersInfo(0x0014, 0, filePath, 0x01 | 0x02);
    }

    private static void SetLinux(string filePath)
    {
        var desktop = Env("XDG_CURRENT_DESKTOP").ToLowerInvariant();
        var session = Env("DESKTOP_SESSION").ToLowerInvariant();
        var fileUri = $"file://{filePath}";

        if (Contains(desktop, session, "gnome", "unity", "budgie", "ubuntu"))
            LinuxGnome(fileUri);
        else if (Contains(desktop, session, "kde", "plasma"))
            LinuxKde(filePath);
        else if (Contains(desktop, session, "xfce"))
            LinuxXfce(filePath);
        else if (Contains(desktop, session, "mate"))
            Run("gsettings", "set", "org.mate.background", "picture-filename", filePath);
        else if (Contains(desktop, session, "cinnamon", "x-cinnamon"))
            Run("gsettings", "set", "org.cinnamon.desktop.background", "picture-uri", fileUri);
        else if (Contains(desktop, session, "lxqt"))
            Run("pcmanfm-qt", "--set-wallpaper", filePath);
        else if (Contains(desktop, session, "lxde"))
            Run("pcmanfm", "--set-wallpaper", filePath);
        else if (Contains(desktop, session, "deepin", "dde"))
            Run("gsettings", "set", "com.deepin.wrap.gnome.desktop.background", "picture-uri", fileUri);
        else if (Contains(desktop, session, "enlightenment"))
            Run("enlightenment_remote", "-desktop-bg-set", "0", "0", "0", "0", filePath);
        else if (Contains(desktop, session, "sway") || !string.IsNullOrEmpty(Env("SWAYSOCK")))
            Run("swaymsg", "output", "*", "bg", filePath, "fill");
        else if (Contains(desktop, session, "hyprland") || !string.IsNullOrEmpty(Env("HYPRLAND_INSTANCE_SIGNATURE")))
            LinuxHyprland(filePath);
        else if (Contains(desktop, session, "i3", "openbox", "fluxbox", "icewm", "jwm"))
            LinuxFehFallback(filePath);
        else
            LinuxAutoDetect(filePath, fileUri);
    }

    private static void LinuxGnome(string fileUri)
    {
        Run("gsettings", "set", "org.gnome.desktop.background", "picture-uri", fileUri);
        Run("gsettings", "set", "org.gnome.desktop.background", "picture-uri-dark", fileUri);
    }

    private static void LinuxKde(string filePath)
    {
        if (!TryRun("plasma-apply-wallpaperimage", filePath))
        {
            var script = $@"var all = desktops(); for (var i = 0; i < all.length; i++) {{ var d = all[i]; d.wallpaperPlugin = 'org.kde.image'; d.currentConfigGroup = ['Wallpaper', 'org.kde.image', 'General']; d.writeConfig('Image', 'file://{filePath}'); }}";
            TryRun("qdbus", "org.kde.plasmashell", "/PlasmaShell", "org.kde.PlasmaShell.evaluateScript", script);
        }
    }

    private static void LinuxXfce(string filePath)
    {
        var output = RunWithOutput("xfconf-query", "-c", "xfce4-desktop", "-l");
        if (output != null)
        {
            bool any = false;
            foreach (var line in output.Split('\n'))
            {
                var prop = line.Trim();
                if (prop.EndsWith("/last-image", StringComparison.Ordinal))
                {
                    Run("xfconf-query", "-c", "xfce4-desktop", "-p", prop, "-s", filePath);
                    any = true;
                }
            }
            if (any) return;
        }
        Run("xfconf-query", "-c", "xfce4-desktop", "-p", "/backdrop/screen0/monitor0/workspace0/last-image", "-s", filePath);
    }

    private static void LinuxHyprland(string filePath)
    {
        if (!TryRun("swww", "img", filePath))
            TryRun("hyprctl", "hyprpaper", "wallpaper", $",{filePath}");
    }

    private static void LinuxFehFallback(string filePath)
    {
        if (!TryRun("feh", "--bg-fill", filePath))
            TryRun("nitrogen", "--set-zoom-fill", filePath);
    }

    private static void LinuxAutoDetect(string filePath, string fileUri)
    {
        if (TryRun("feh", "--bg-fill", filePath)) return;
        if (TryRun("nitrogen", "--set-zoom-fill", filePath)) return;
        if (TryRun("gsettings", "set", "org.gnome.desktop.background", "picture-uri", fileUri)) return;
        TryRun("plasma-apply-wallpaperimage", filePath);
    }

    private static bool Contains(string desktop, string session, params string[] keywords)
    {
        foreach (var kw in keywords)
            if (desktop.Contains(kw) || session.Contains(kw))
                return true;
        return false;
    }

    private static string Env(string key) => Environment.GetEnvironmentVariable(key) ?? string.Empty;

    private static void Run(string cmd, params string[] args)
    {
        try
        {
            var psi = new ProcessStartInfo(cmd) { UseShellExecute = false };
            foreach (var a in args) psi.ArgumentList.Add(a);
            Process.Start(psi)?.WaitForExit();
        }
        catch { }
    }

    private static bool TryRun(string cmd, params string[] args)
    {
        try
        {
            var psi = new ProcessStartInfo(cmd) { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
            foreach (var a in args) psi.ArgumentList.Add(a);
            var p = Process.Start(psi);
            p?.WaitForExit();
            return p?.ExitCode == 0;
        }
        catch { return false; }
    }

    private static string? RunWithOutput(string cmd, params string[] args)
    {
        try
        {
            var psi = new ProcessStartInfo(cmd) { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
            foreach (var a in args) psi.ArgumentList.Add(a);
            var p = Process.Start(psi);
            var output = p?.StandardOutput.ReadToEnd();
            p?.WaitForExit();
            return p?.ExitCode == 0 ? output : null;
        }
        catch { return null; }
    }
}
