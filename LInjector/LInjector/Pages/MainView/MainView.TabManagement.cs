using LInjector.Classes;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace LInjector.Pages
{
    public partial class MainView
    {
        private void TabSystem__Loaded(object sender, RoutedEventArgs e) => LoadSavedTabs();

        private async Task SaveTabs()
        {
            if (SettingsWrapper.Read("save_tabs") == true)
            {
                foreach (TabItem item in TabSystem_.maintabs.Items)
                {
                    var Text = await (item.Content as MonacoApi)!.GetText();
                    if (!string.IsNullOrEmpty(Text))
                        if (!Directory.Exists(Strings.Get("Saved")))
                            Directory.CreateDirectory(Strings.Get("Saved"));

                    File.WriteAllText($"{Strings.Get("Saved")}\\{item.Header.ToString()}", Text);
                }
            }
        }

        public void LoadSavedTabs()
        {
            if (SettingsWrapper.Read("save_tabs") == false)
                return;

            foreach (string file in Directory.EnumerateFiles(Strings.Get("Saved")))
                TabSystem_.Add_tab_with_text(File.ReadAllText(file), Path.GetFileName(file));

            DeleteSavedTabs();
        }

        public void DeleteSavedTabs()
        {
            foreach (var item in Directory.EnumerateFiles(Strings.Get("Saved")))
                File.Delete(item);
        }

        private async void EditorButtonClick(object sender, RoutedEventArgs e)
        {
            string action = (sender as FrameworkElement)!.Name;
            switch (action)
            {
                case "ExecuteButtonMain":
                    try
                    {
                        var cm = TabSystem_.CurrentMonaco();

                        string scriptString = await cm!.GetText();

                        try
                        {
                            var flag = !DLLInterface.IsAttached();
                            if (!flag)
                            {
                                DLLInterface.RunScript(scriptString);
                            }
                            else
                            {
                                DLLInterface.Inject();
                                await Task.Delay(500);
                                DLLInterface.RunScript(scriptString);
                            }
                        }
                        catch (Exception ex)
                        {
                            await Logs.Console($"An exception has occurred\n{ex.Message}\n{ex.StackTrace}");
                        }
                    }
                    catch
                    {
                        await Logs.Console("Unknown error");
                    }
                    break;
                case "OpenFileButton":
                    Show_OpenFileDialog();
                    break;
                case "SaveFileButton":
                    Show_SaveToFileDialog();
                    break;
                default: break;
            }
        }
    }
}