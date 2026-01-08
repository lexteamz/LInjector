using System.IO;
using System.Windows.Threading;

namespace LInjector.Classes
{
    public class ScriptContext
    {
        static string path = Path.Combine(Path.GetTempPath(), "LInjector", "Init.lua");

        public static async Task BeginFunctionTick()
        {
            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1000)
            };

            timer.Tick += (sender, e) =>
            {
                if (!DLLInterface.IsAttached())
                    return;

                if ((bool)SettingsWrapper.Read("safe_mode"))
                    return;

                string initContent = File.ReadAllText(path);
                DLLInterface.RunScript(initContent);
            };

            timer.Start();
        }

        public static async Task EnsureAndPutAutoExec()
        {
            string autoexecDir = Path.Combine(Strings.Get("AppRoot"), "autoexec");
            string autoexecFile = Path.Combine(autoexecDir, "Init.lua");

            if (!Directory.Exists(autoexecDir))
                Directory.CreateDirectory(autoexecDir);

            await EnsureFunctionsFile();

            if (!File.Exists(autoexecFile))
                await File.WriteAllTextAsync(autoexecFile, File.ReadAllText(path));
            else if ((bool)SettingsWrapper.Read("safe_mode"))
                File.Delete(autoexecFile);
        }

        public static async Task EnsureFunctionsFile()
        {
            if (File.Exists(path))
                return;

            await ResourceManager.DownloadFileToTempAsync(
                "https://raw.githubusercontent.com/LExteamz/LInjector/refs/heads/main/LInjector/LInjector/Resources/Internal/Init.lua",
                "Init.lua"
            );
        }
    }
}
