using System.Diagnostics;

namespace LInjector.Classes
{
    public static class DLLInterface
    {
        public static async void Inject()
        {
            try
            {
                if (Process.GetProcessesByName("RobloxPlayerBeta").Length <= 0)
                {
                    await Logs.Console("Please, open Roblox", true);
                }
                else
                {
                    // Your Inject Logic

                    await Logs.Console("Injected", true);
                }
            }
            catch (Exception ex)
            {
                await Logs.Console($"Exception has occurred:\n{ex.Message}\n{ex.StackTrace}");
            }
        }


        public static bool IsAttached()
        {
            // Your IsAttached Logic

            return false;
        }

        public static bool RunScript(string src)
        {
            if (!IsAttached())
                return false;

            // Your RunScript Logic

            return true;
        }

    }
}
