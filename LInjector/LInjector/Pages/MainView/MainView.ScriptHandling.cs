using LInjector.Classes;
using LInjector.Pages.Popups;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LInjector.Pages
{
    public partial class MainView
    {
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref System.Drawing.Point lpPoint);

        private static string regScriptValue = RegistryHandler.GetValue("ScriptListPath", "0");
        private static string ScriptListPath = string.IsNullOrEmpty(regScriptValue) ? Path.Combine(Strings.Get("AppRoot"), "scripts") : regScriptValue;

        public class ScriptListItem
        {
            public string? FileName { get; set; }
            public bool? IsFavourite { get; set; }
        }

        private void ScriptSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => RefreshScriptList();

        public void RefreshScriptList()
        {
            if (!Directory.Exists(ScriptListPath)) return;

            string searchQuery = ScriptSearchBox.Text?.ToLower() ?? "";
            DirectoryInfo scriptsFolder = new(ScriptListPath);

            var favToken = SettingsWrapper.Read("favourited_scripts") as JArray;
            var favList = favToken?.Select(t => t.ToString()).ToList() ?? new List<string>();

            var items = scriptsFolder.GetFiles()
                .Select(f => new ScriptListItem
                {
                    FileName = f.Name,
                    IsFavourite = favList.Contains(f.Name)
                })
                .Where(item => string.IsNullOrEmpty(searchQuery) || item.FileName!.ToLower().Contains(searchQuery))
                .OrderByDescending(item => item.IsFavourite)
                .ThenBy(item => item.FileName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            NormalStandaloneScriptsHolder.ItemsSource = items;

            var dir = new DirectoryInfo(ScriptListPath);
            ScriptDirLabel.Content = dir.FullName.Contains(Strings.Get("AppRoot")) ? $"root/{dir.Name}" : dir.Name;
        }

        private void NormalStandaloneScriptsHolder_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(ScriptListPath))
            {
                ScriptListPath = Path.Combine(Strings.Get("AppRoot"), "scripts");
                Directory.CreateDirectory(ScriptListPath);
                File.WriteAllText(Path.Combine(ScriptListPath, "example.lua"), "print(\"LInjector v3, yay!\")");
            }
            RefreshScriptList();
        }

        private void NormalStandaloneScriptsHolder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (NormalStandaloneScriptsHolder.SelectedItem is ScriptListItem selectedScript)
                {
                    string scriptPath = Path.Combine(ScriptListPath, selectedScript.FileName!);
                    if (!File.Exists(scriptPath)) return;

                    string content = File.ReadAllText(scriptPath);
                    if (TabSystem_.maintabs.Items.Count == 0)
                        TabSystem_.Add_tab_with_text(content, selectedScript.FileName);
                    else
                    {
                        TabSystem_.ChangeCurrentTabTitle(selectedScript.FileName!);
                        TabSystem_.CurrentMonaco()?.SetText(content);
                    }
                    TabSystem_.Focus();
                }
                NormalStandaloneScriptsHolder.SelectedItem = null;
            }
            catch { }
        }

        private void ToggleFavouriteItem(FrameworkElement bozo) => bozo.Tag = bozo.Tag?.ToString()?.Contains("favourite") == true ? null : "favourite";

        private void Item_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void Item_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var item = sender as ListBoxItem;
            if (item?.DataContext is ScriptListItem scriptItem)
            {
                string action = (bool)scriptItem.IsFavourite! ? "Unfavourite" : "Favourite";

                var options = new[]
                {
            new LIContextMenuStrip.MenuItemOption(action, "\ue734", (s, ev) =>
            {
                var favToken = SettingsWrapper.Read("favourited_scripts") as JArray;
                var favList = favToken?.Select(t => t.ToString()).ToList() ?? new List<string>();

                if ((bool)scriptItem.IsFavourite!)
                    favList.Remove(scriptItem.FileName!);
                else
                    favList.Add(scriptItem.FileName!);

                SettingsWrapper.Write("favourited_scripts", JArray.FromObject(favList));

                RefreshScriptList();
            })
        };

                System.Drawing.Point defPnt = new();
                GetCursorPos(ref defPnt);
                LIContextMenuStrip.ShowMenu(defPnt, options);
            }
        }

        private void ChangeScriptListFolder(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Multiselect = false,
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                RegistryHandler.SetValue("ScriptListPath", dialog.FileName);
                ScriptListPath = dialog.FileName;
                RefreshScriptList();
            }
        }

        public async void Show_SaveToFileDialog()
        {
            var saveFileDialog = new System.Windows.Forms.SaveFileDialog
            {
                FileName = await TabSystem_.GetCurrentTabTitle(),
                Title = "Save to File | LInjector",
                Filter = "Script Files (*.txt;*.lua;*.luau)|*.txt;*.lua;*.luau|All files (*.*)|*.*",
            };

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;

                try
                {
                    var cm = TabSystem_.CurrentMonaco();
                    string text = await cm!.GetText();
                    string result = text;

                    if (string.IsNullOrEmpty(result))
                        await Logs.Console("No content detected");
                    else
                    {
                        try
                        {
                            File.WriteAllText(filePath, result);
                            string savedFileName = Path.GetFileName(saveFileDialog.FileName);
                            await Logs.Console($"{savedFileName} saved");
                            TabSystem_.ChangeCurrentTabTitle(savedFileName);
                        }
                        catch (Exception)
                        {
                            await Logs.Console("Error saving the file");
                        }
                    }

                }
                catch (Exception)
                {
                    await Logs.Console("Error saving the file");
                }
            }
        }

        public async void Show_OpenFileDialog()
        {
            try
            {
                var openFileDialog = new System.Windows.Forms.OpenFileDialog
                {
                    Title = $"Open Script Files | {Strings.Get("AppName")}",
                    Filter = "Script Files (*.txt;*.lua;*.luau)|*.txt;*.lua;*.luau|All files (*.*)|*.*",
                    Multiselect = false
                };

                if (Directory.Exists(ScriptListPath))
                    openFileDialog.InitialDirectory = ScriptListPath;

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string fileContent = File.ReadAllText(openFileDialog.FileName);

                    var dialogResult = System.Windows.Forms.MessageBox.Show(
                        "Open file in new tab?", $"{Strings.Get("AppName")}", System.Windows.Forms.MessageBoxButtons.YesNo,
                        System.Windows.Forms.MessageBoxIcon.Question, System.Windows.Forms.MessageBoxDefaultButton.Button2);
                    if (dialogResult == System.Windows.Forms.DialogResult.Yes)
                    {
                        TabSystem_.Add_tab_with_text(fileContent, openFileDialog.SafeFileName);
                    }
                    else
                    {
                        TabSystem_.CurrentMonaco()!.SetText(fileContent);
                        TabSystem_.ChangeCurrentTabTitle(openFileDialog.SafeFileName);
                    }
                }

            }
            catch
            {
                await Logs.Console("Error while opening the file.");
            }
        }
    }
}