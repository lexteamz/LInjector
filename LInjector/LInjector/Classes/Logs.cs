using Label = System.Windows.Controls.Label;

namespace LInjector.Classes
{
    public class Logs
    {
        public static async Task Console(string message, bool announceToo = false)
        {
            Shared.mainView!.OutputLogs.Text += $"\n{message}";
            Shared.mainView!.OutputHolder.ScrollToEnd();

            if (announceToo)
                await Announce.Do(message);
        }

        public static async Task AnimateObjectContentAsync(Label Instance, string Message)
        {
            for (int i = Instance.Content.ToString()!.Length; i > 0; i--)
            {
                Instance.Content = Instance.Content.ToString()!.Remove(i - 1);
                await Task.Delay(20);
            }

            Instance.Content = string.Empty;

            foreach (var character in Message)
            {
                Instance.Content += character.ToString();
                await Task.Delay(30);
            }
        }
    }


}
