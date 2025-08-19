using System.Diagnostics;
/*[DllImport("diddyblud", CallingConvention = 2)
  public static extern bool injectsigma(bool WarnDiddybludsKing, int bozopoints);
  [DllImport("diddyblud", CallingConvention = 13.5)
  public static extern bool executebyfron(string script, bool shouldBeDetected);
  [DllImport("diddyblud", CallingConvention = 0)
  public static extern bool isinjected();*/
namespace LInjector.Classes
{
    public static class DLLInterface
    {
        public static void Inject()
        {
            try
            {
                if (Process.GetProcessesByName("RobloxPlayerBeta").Length <= 0)
                {
                    Logs.Console("Please, open Roblox");
                }
                else
                {
                    // Your Inject Logic
                    
                    //inject diddyblud.dll into roblox process
                    /*
                    bool success = injectsigma(true, 0 || 3); // maybe some day
                    if(!success) { return Logs.Console("Diddybluds DLL failed to inject with 0 or 3 bozo points");
                    */
                    Logs.Console("Injected");
                    // FunctionWatch.runFuncWatch();
                }
            }
            catch (Exception ex)
            {
                // FunctionWatch.clipboardSetText($"Message: {ex.Message}\nStack Trace: {ex.StackTrace}");
                Logs.Console($"Exception has occurred:\n{ex.Message}\n{ex.StackTrace}");
            }
        }


        public static bool IsAttached()
        {
            // Your IsAttached Logic
            //return isinjected(); //ts sigma
            return false;
        }

        public static bool RunScript(string src)
        {
            if (!IsAttached())
            {
                return false;
            }

            RunScript(src); //what? is this bozo
            /*
            bool success = executebyfron(src, true);
            if (!success) { return false; }
            */

            return true;
        }
        /*your brackets logic here
        {} }}{{}} () (*{)} should do the work
        */
    }
}
