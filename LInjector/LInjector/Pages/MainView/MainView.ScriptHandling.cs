using LInjector.Classes;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace LInjector.Pages
{
    public partial class MainView
    {
        private static string regScriptValue = RegistryHandler.GetValue("ScriptListPath", "0");
        private static string ScriptListPath = string.IsNullOrEmpty(regScriptValue) ? Path.Combine(Strings.Get("AppRoot"), "scripts") : regScriptValue;

        private void ScriptSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => RefreshScriptList();

        public void RefreshScriptList()
        {
            NormalStandaloneScriptsHolder.Items.Clear();

            string searchQuery = ScriptSearchBox.Text.ToLower();
            DirectoryInfo scriptsFolder = new(ScriptListPath);

            foreach (FileInfo script in scriptsFolder.GetFiles())
            {
                string fileName = script.Name;
                if (fileName.Contains(searchQuery, StringComparison.CurrentCultureIgnoreCase))
                {
                    NormalStandaloneScriptsHolder.Items.Add(fileName);
                }
            }

            var dir = new DirectoryInfo(ScriptListPath);
            if (dir.FullName.Contains(Strings.Get("AppRoot")))
                ScriptDirLabel.Content = $"root/{dir.Name}";
            else
                ScriptDirLabel.Content = dir.Name;
        }

        private void NormalStandaloneScriptsHolder_Loaded(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(ScriptListPath))
            {
                DirectoryInfo ScriptsFolder = new(ScriptListPath);
                FileInfo[] Files = ScriptsFolder.GetFiles("*.*");
                foreach (FileInfo Script in Files)
                {
                    NormalStandaloneScriptsHolder.Items.Add(Script.Name);
                }

                ScriptDirLabel.Content = new DirectoryInfo(ScriptListPath).Name;
            }
            else
            {
                ScriptListPath = Path.Combine(Strings.Get("AppRoot"), "scripts");

                if (!Directory.Exists(ScriptListPath))
                {
                    Directory.CreateDirectory(ScriptListPath);
                    File.WriteAllText(Path.Combine(ScriptListPath, "example.lua"), "print(\"LInjector v3, yay!\")");

                    RefreshScriptList();
                }
            }
        }

        private void NormalStandaloneScriptsHolder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (TabSystem_.maintabs.Items.Count != 0)
                {
                    if (this.NormalStandaloneScriptsHolder.SelectedIndex != -1)
                    {
                        string scriptfolder = ScriptListPath;
                        object selectedItem = NormalStandaloneScriptsHolder.SelectedItem;
                        if (this.NormalStandaloneScriptsHolder.Items.Count != 0)
                        {
                            TabSystem_.ChangeCurrentTabTitle(selectedItem.ToString()!);
                            TabSystem_.CurrentMonaco()!.SetText(File.ReadAllText(scriptfolder + "\\" + (selectedItem?.ToString())));
                        }
                    }
                }
                else
                {
                    string scriptfolder = ScriptListPath;
                    object selectedItem = NormalStandaloneScriptsHolder.SelectedItem;
                    TabSystem_.Add_tab_with_text(File.ReadAllText(scriptfolder + "\\" + (selectedItem?.ToString())), selectedItem!.ToString());
                }

                TabSystem_.Focus();
                NormalStandaloneScriptsHolder.SelectedItem = null;
            }
            catch { }
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
                    Title = $"Open Script Files | {Strings.Get("AppVersion")}",
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