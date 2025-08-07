using System.IO;
using System.Media;
using System.Net;

namespace LInjector.Classes
{
    public static class StartupHandler
    {

        private static readonly string baseUrl = "https://short.lexploits.top";
        private static readonly string metalpipeURL = $"{baseUrl}/Assets/40506d549f23856071e7beed4b35c097.wav";
        private static readonly string bamboopipeURL = $"{baseUrl}/Assets/40e0cc9d289d38f0acfeb076eeb785eb.wav";
        private static readonly string windowsURL = $"{baseUrl}/Assets/0f4137ed1502b5045d6083aa258b5c42.wav";

        private static readonly string TempPath = Path.Combine(Path.GetTempPath(), Strings.Get("AppName"));

        private static async Task<string> DownloadFileAsync(string url)
        {
            Directory.CreateDirectory(TempPath);
            string filePath = Path.Combine(TempPath, Path.GetFileName(url));

            using (WebClient client = new WebClient())
            {
                await client.DownloadFileTaskAsync(new Uri(url), filePath);
            }

            return filePath;
        }

        private static void PlaySound(string filePath)
        {
            try
            {
                using (var player = new SoundPlayer(filePath))
                {
                    player.Play();
                }
            }
            catch { }
        }

        public static async void PlayStartupSound(string sound)
        {
            string url = null!;

            switch (sound)
            {
                case "metal":
                    url = metalpipeURL;
                    break;
                case "bamboo":
                    url = bamboopipeURL;
                    break;
                case "windows":
                    url = windowsURL;
                    break;
                default:
                    return;
            }

            if (url != null)
            {
                string filePath = Path.Combine(TempPath, Path.GetFileName(url));

                if (!File.Exists(filePath))
                {
                    filePath = await DownloadFileAsync(url);
                }

                PlaySound(filePath);
            }
        }



    }

}
