using System.Diagnostics;

namespace WallpaperPicker.Services.Setters;

static class WallpaperHelper
{
    public static bool Contains(string desktop, string session, params string[] keywords)
    {
        foreach (var kw in keywords)
            if (desktop.Contains(kw) || session.Contains(kw))
                return true;
        return false;
    }

    public static string Env(string key) =>
        Environment.GetEnvironmentVariable(key) ?? string.Empty;

    public static (bool Ok, string? Output) Exec(string cmd, params string[] args)
    {
        try
        {
            var psi = new ProcessStartInfo(cmd) { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
            foreach (var a in args) psi.ArgumentList.Add(a);
            var p = Process.Start(psi);
            var output = p?.StandardOutput.ReadToEnd();
            p?.WaitForExit();
            return (p?.ExitCode == 0, output);
        }
        catch { return (false, null); }
    }
}
