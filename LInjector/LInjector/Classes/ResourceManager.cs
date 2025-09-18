using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;

namespace LInjector.Classes
{
    public class ResourceManager
    {
        [DllImport("gdi32.dll", EntryPoint = "AddFontResourceW", SetLastError = true)]
        public static extern int AddFontResource([In][MarshalAs(UnmanagedType.LPWStr)] string lpFileName);

        [DllImport("user32.dll", EntryPoint = "SendMessageTimeoutW", SetLastError = true)]
        public static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam, uint fuFlags, uint uTimeout, out IntPtr lpdwResult);


        private const uint WM_FONTCHANGE = 0x001D;
        private const uint SMTO_ABORTIFHUNG = 0x0002;

        public static bool IsFontInstalled(string fontName) => (new InstalledFontCollection()).Families.Any(f => f.Name.Equals(fontName, StringComparison.OrdinalIgnoreCase));

        public static void InstallFont(string fontFilePath)
        {
            int result = AddFontResource(fontFilePath);

            if (result == 0)
                return;

            IntPtr hWndBroadcast = new IntPtr(0xFFFF); // HWND_BROADCAST
            IntPtr lpdwResult;
            SendMessageTimeout(hWndBroadcast, WM_FONTCHANGE, IntPtr.Zero, IntPtr.Zero, SMTO_ABORTIFHUNG, 1000, out lpdwResult);
        }

        public static async Task<string> DownloadFileToTempAsync(string url, string fileName)
        {
            if (string.IsNullOrEmpty(url))
                Debug.WriteLine("URL cannot be null or empty.", nameof(url));

            if (!Directory.Exists(Path.Combine(Path.GetTempPath(), $"{Strings.Get("AppName")}")))
                Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"{Strings.Get("AppName")}"));

            if (string.IsNullOrEmpty(fileName))
                Debug.WriteLine("File name cannot be null or empty.", nameof(fileName));

            string tempFilePath = Path.Combine(Path.GetTempPath(), $"{Strings.Get("AppName")}", fileName);

            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    byte[] newFileData = await httpClient.GetByteArrayAsync(url);

                    if (File.Exists(tempFilePath))
                    {
                        byte[] existingFileData = await File.ReadAllBytesAsync(tempFilePath);
                        if (existingFileData.SequenceEqual(newFileData))
                        {
                            return tempFilePath;
                        }
                    }

                    await File.WriteAllBytesAsync(tempFilePath, newFileData);
                    return tempFilePath;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return string.Empty;
        }
    }
}
