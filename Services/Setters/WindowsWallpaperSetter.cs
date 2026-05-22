using System.Runtime.InteropServices;

namespace WallpaperPicker.Services.Setters;

class WindowsWallpaperSetter : BaseWallpaperSetter
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

    public override bool Set(string filePath)
    {
        SystemParametersInfo(0x0014, 0, filePath, 0x01 | 0x02);
        return true;
    }
}
