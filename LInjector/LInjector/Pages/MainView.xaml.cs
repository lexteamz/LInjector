using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using Dsafa.WpfColorPicker;
using LInjector.Classes;
using LInjector.Pages.Elements;
using LInjector.Pages.Popups;
using LInjector.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using Color = System.Windows.Media.Color;
using File = System.IO.File;

namespace LInjector.Pages
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView
    {

        public MainView()
        {
            InitializeComponent();
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child!))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private void PlayRandomSound(object sender, RoutedEventArgs e)
        {
            int rand = new Random().Next(1, Splash.soundEvents.Length);
            string RandomEvent = Splash.soundEvents[rand];

            StartupHandler.PlayStartupSound(RandomEvent);
        }


        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Section_Settings.Visibility = Visibility.Hidden;
            Section_Editor.Visibility = Visibility.Hidden;

            TitleBarLabel.Content = $"{Strings.Get("AppName")}";

            TabSystem_.Visibility = Visibility.Visible;
            TabSystem_.IsEnabled = true;

            ParseMyTheme();
            ParseConfig();
            ParseMyThemeSelectors();
            Announce.InitVars(the_bocchler, dorito);

            NavigationGridClick(Editor, e);

            await ScriptContext.EnsureFunctionsFile();
            await ScriptContext.BeginFunctionTick();
            BeginAttachDetection();

            if (DateTime.Now.Month == 4 && DateTime.Now.Day == 1) // April 1st
            {
                foreach (var button in FindVisualChildren<Button>(this))
                {
                    button.Click += PlayRandomSound;
                }
            }

            Shared.mainWindow!.MouseMove += GlMouseMove;
            Shared.mainWindow!.MouseUp += GlWndMouseUp;
        }

        public void ApplyConfig(object sender, RoutedEventArgs e)
        {
            // Window Opacity
            double wndOpacity = double.Parse(Themes.GetColor("WindowOpacity"));
            Shared.mainWindow!.Opacity = wndOpacity;
            foreach (TabItem item in TabSystem_.maintabs.Items)
            {
                MonacoApi TabInstance = (item.Content as MonacoApi)!;
                TabInstance.ChangeEditorTransparency(wndOpacity);
            }
            OpacityValueIndicator.Content = $"{wndOpacity.ToString("F2")}";

            // Set Background Image
            string backgroundLocation = Themes.GetColor("BackgroundLocation");
            WindowBackgroundImage.Source = backgroundLocation.StartsWith("http")
                ? new System.Windows.Media.Imaging.BitmapImage(new Uri(backgroundLocation))
                : new System.Windows.Media.Imaging.BitmapImage(new Uri(backgroundLocation, UriKind.RelativeOrAbsolute));

            // Load Background Blur Radius
            double backgroundBlurRadius = double.Parse(Themes.GetColor("BackgroundBlurRadius"));
            WindowBackgroundBlur.Radius = backgroundBlurRadius;
            OpacityBackBlurValueIndicator.Content = $"{backgroundBlurRadius.ToString()}";

            // Load Background Opacity
            double backgroundOpacity = double.Parse(Themes.GetColor("BackgroundOpacity"));
            WindowBackgroundImage.Opacity = backgroundOpacity;
            OpacityBackValueIndicator.Content = $"{backgroundOpacity.ToString("F2")}";

            // Ellipse Radial only uses Topbar space
            var currentParent = sixty_seven.Parent as Grid;

            if (SettingsWrapper.Read("ellipse_topbar") == true)
            {
                if (currentParent != oneeightzerozero)
                {
                    currentParent?.Children.Remove(sixty_seven);
                    oneeightzerozero.Children.Add(sixty_seven);
                }
            }
            else
            {
                if (currentParent != heheheha)
                {
                    currentParent?.Children.Remove(sixty_seven);
                    heheheha.Children.Add(sixty_seven);
                }
            }


            // Hide from Capture
            Shared.SetWindowCaptureProtection(new WindowInteropHelper(Shared.mainWindow!).Handle, (bool)SettingsWrapper.Read("hide_capture"));

            // Top Most
            Shared.mainWindow!.Topmost = (bool)(SettingsWrapper.Read("top_most"));

            // Show Script List
            Shared.mainView!.ScriptListDimensions.Width = ((bool)SettingsWrapper.Read("show_scriptlist")) ? new GridLength(120, GridUnitType.Star) : new GridLength(0, GridUnitType.Pixel);

            // Show Logs
            bool showConsole = (bool)SettingsWrapper.Read("show_internalconsole");

            // Configure console dimensions
            Shared.mainView.LInjectorConsoleDimensions.Height = new GridLength(
                showConsole ? 140 : 0,
                showConsole ? GridUnitType.Star : GridUnitType.Pixel
            );

            // Configure editor border
            Shared.mainView.EditorBorder.BorderThickness = new Thickness(
                0, 0, 0,
                showConsole ? 0.1 : 0
            );

            // Editor Transparency if override
            double editorOpacityValue = double.Parse(Themes.GetColor("EditorTransparency_Override"));
            foreach (TabItem item in TabSystem_.maintabs.Items)
            {
                MonacoApi TabInstance = (item.Content as MonacoApi)!;
                TabInstance.ChangeEditorTransparency(wndOpacity != 1 ? wndOpacity : editorOpacityValue);
            }
            EditorTransparencyValueIndicator.Content = $"{editorOpacityValue.ToString("F2")}";

            // Auto Attach
            RunAutoAttachTimer();

            // Discord RPC
            BeginDiscordRPC();

            // Editor
            bool isBlurred = SettingsWrapper.Read("editor_blurred") == true;
            bool showMinimap = SettingsWrapper.Read("show_minimap") == true;
            foreach (TabItem item in TabSystem_.maintabs.Items)
            {
                MonacoApi TabInstance = (item.Content as MonacoApi)!;

                // Theme
                TabInstance.SetTheme($"\"{(SettingsWrapper.Read("monaco_theme")!.ToObject<string[]>())[0]}\"");

                if (isBlurred) TabInstance.EnableBlur(); else TabInstance.DisableBlur();

                if (showMinimap) TabInstance.EnableMinimap(); else TabInstance.DisableMinimap();
            }
        }

        private void ParseConfig()
        {
            var configInitElement = SettingsWrapper.Read("app_init");

            if (configInitElement == false)
            {
                SettingsWrapper.SetDefaultConfig();
                SettingsWrapper.Write("app_init", true);
            }

            Factory factory = new Factory(SettingsItemsHolder);
            var config = JsonConvert.DeserializeObject<Dictionary<string, JToken>>(File.ReadAllText(SettingsWrapper._configFilePath));

            foreach (var x in config!)
            {
                switch (SettingsWrapper.Read(x.Key))
                {
                    case JValue boolValue when boolValue.Type == JTokenType.Boolean:
                        factory.CreateCheckboxOption((Action<bool>)((x) => { }), boolValue.ToObject<bool>(), x.Key);
                        break;

                    case JValue stringValue when stringValue.Type == JTokenType.String:
                        factory.CreateTextFieldOption((Action<string>)((x) => { }), stringValue.ToString(), false, x.Key);
                        break;

                    case JArray stringArray:
                        var array = (stringArray.ToObject<string[]>())!.ToArray();
                        factory.CreateDropdownOption((Action<string[]>)((x) => { }), array, new[] { "", "", "" }, x.Key, x.Key);
                        break;

                    case JValue numericValue when numericValue.Type == JTokenType.Integer || numericValue.Type == JTokenType.Float:
                        factory.CreateTextFieldOption((Action<string>)((x) => { }), numericValue.ToString(), true, x.Key);
                        break;

                    default:
                        break;
                }
            }

            VersionPlaceHolderCredits.Text = $"{Strings.Get("AppName")} {Strings.Get("AppVersion")}";

            ApplyConfig(null!, null!);
        }

        DispatcherTimer TitleTimer = new DispatcherTimer();
        DispatcherTimer bozoTimer = new DispatcherTimer();
        DispatcherTimer Discordtimer = new DispatcherTimer(DispatcherPriority.Background);

        public void BeginDiscordRPC()
        {
            Discordtimer.Interval = TimeSpan.FromSeconds(5);
            Discordtimer.Tick += DiscordRPCTick!;
            Discordtimer.Start();

        }
        internal void DiscordRPCTick(object sender, EventArgs e)
        {
            if (SettingsWrapper.Read("discord_rpc") == true)
            {
                RPCManager.isEnabled = true;
                if (!RPCManager.client!.IsInitialized)
                    RPCManager.InitRPC();
            }
            else
            {
                RPCManager.isEnabled = false;
                if (RPCManager.client!.IsInitialized)
                    RPCManager.TerminateConnection();
            }
        }

        public void BeginAttachDetection()
        {
            bozoTimer.Interval = TimeSpan.FromSeconds(1);
            bozoTimer.Tick += bozoTick!;
            bozoTimer.Start();
        }

        public void RunAutoAttachTimer()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += AttachedDetectorTick!;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();
        }

        internal void AttachedDetectorTick(object sender, EventArgs e)
        {
            if (SettingsWrapper.Read("auto_attach") == false) return;

            var processesByName = Process.GetProcessesByName("RobloxPlayerBeta");
            foreach (var process in processesByName)
            {
                try
                {
                    var filePath = process.MainModule!.FileName;

                    if (DLLInterface.IsAttached())
                        return;

                    DLLInterface.Inject();
                }
                catch (Exception ex)
                {
                    ConsoleControl.Log(ex.Message);
                }
            }
        }

        public void AnimateColor(DropShadowEffect element, Color final, double duration = 1)
        {
            var currentColor = element.Color;
            if (currentColor.Equals(final)) return;


            var colorAnim = new ColorAnimation
            {
                From = currentColor,
                To = final,
                Duration = new Duration(TimeSpan.FromSeconds(duration)),
                FillBehavior = FillBehavior.HoldEnd
            };

            element.BeginAnimation(DropShadowEffect.ColorProperty, colorAnim);
        }

        public void AnimateBlur(DropShadowEffect element, double target, double duration = 1)
        {
            if (Math.Abs(target - element.BlurRadius) < 0.01) return;

            var blurRadiusAnim = new DoubleAnimation
            {
                To = target,
                Duration = new Duration(TimeSpan.FromSeconds(duration)),
                FillBehavior = FillBehavior.HoldEnd
            };

            element.BeginAnimation(DropShadowEffect.BlurRadiusProperty, blurRadiusAnim);
        }

        public void bozoTick(object sender, EventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            bool isAttached = DLLInterface.IsAttached() /* || Shared.ws.GetDevicesConnected() > 0 */;

            AnimateColor(HarderBetterFasterStronger, ConsoleControl.ParseColor(isAttached ? "#FF7B68EE" : "#FF000000").Color);
            AnimateBlur(HarderBetterFasterStronger, isAttached ? 30 : 15);
        }

        public void TitleBarLabel_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this)) { return; }

            int seconds = new Random().Next(45, 60);

            TitleTimer.Interval = TimeSpan.FromSeconds(seconds);
            TitleTimer.Tick += randomizeTitleEvent!;
            TitleTimer.Start();
        }

        public void randomizeTitleEvent(object sender, EventArgs e) => randomizeTitle();

        // ---------------------------- ATTENTION -------------------------------
        // ----------------------------------------------------------------------
        // THIS FUNCTION IS JUST A JOKE, IT'S NOT TO PRETEND TO BE OTHER PROGRAMS
        // ----------------------------------------------------------------------
        public async void randomizeTitle()
        {
            string[] ArrayTitles = {
                "LInjector",
                "x64dbg",
                "IDA Freeware",
                "HxD Editor",
                "Cheat Engine 7.5",
                "IDA Pro",
                "Ghidra",
                "Radare2",
                "Binary Ninja",
                "ReClass.NET",
                "WinDbg",
                "PE Tools",
                "PE Explorer",
                "Dumping in Progress..."
            };

            Random random = new Random();
            int randomIndex = random.Next(0, ArrayTitles.Length);
            string randomTitle = ArrayTitles[randomIndex];

            await Logs.AnimateObjectContentAsync(TitleBarLabel, randomTitle);
        }

        private void DragWnd(object sender, MouseButtonEventArgs e) => Shared.DragWnd();

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

        private void GitHub_onClick(object sender, RoutedEventArgs e) => Shared.OpenURL(Strings.Get("GitHubURL"));

        private void Discord_MouseDown(object sender, MouseButtonEventArgs e) => Shared.OpenURL(Strings.Get("DiscordServerURL"));

        public void OnCloseFadeoutCompleted(object sender, EventArgs e)
        {
            // await Shared.ws.CloseWebSocket();
            Shared.mainWindow!.Close();
            Application.Current.Shutdown();
        }

        private async void ContextMenuClick(object sender, RoutedEventArgs e)
        {
            string action = (sender as FrameworkElement)!.Name;
            switch (action)
            {
                case "Minimise":
                    Shared.mainWindow!.WindowState = WindowState.Minimized;
                    break;

                case "Restore":
                    Shared.mainWindow!.WindowState =
                        Shared.mainWindow!.WindowState == WindowState.Normal
                            ? WindowState.Maximized
                            : WindowState.Normal;
                    break;

                case "Close":
                    await SaveTabs();
                    TabSystem_.Visibility = Visibility.Collapsed;

                    var fadeOutAnimation = new DoubleAnimation { From = 1, To = 0, Duration = TimeSpan.FromSeconds(0.1) };

                    Storyboard fadeOutStoryboard = new();
                    fadeOutStoryboard.Children.Add(fadeOutAnimation);
                    Storyboard.SetTarget(fadeOutAnimation, this);
                    Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath(OpacityProperty));
                    fadeOutStoryboard.Completed += OnCloseFadeoutCompleted!;
                    fadeOutStoryboard.Begin();
                    break;

                default:
                    break;
            }
        }

        private void NavigationGridClick(object sender, RoutedEventArgs e)
        {
            FrameworkElement? element = sender as FrameworkElement;
            if (element == null) return;

            ResetBorderAndOpacity(Settings);
            ResetBorderAndOpacity(Editor);

            var actions = new Dictionary<string, Action>
            {
                { "Editor", () => ApplyBorderAnimation(Editor) },
                { "Settings", () => ApplyBorderAnimation(Settings) }
            };

            if (actions.TryGetValue(element.Name, out var action))
            {
                action.Invoke();

                foreach (Grid grid in m_grid.Children.OfType<Grid>())
                {
                    if (grid.Name.StartsWith("Section_"))
                    {
                        grid.Visibility = grid.Name == $"Section_{element.Name}" ? Visibility.Visible : Visibility.Collapsed;
                    }
                }

                // Logs.Console(element.Name);
            }
        }

        private IEnumerable<T> FindDescendants<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T descendant)
                    yield return descendant;

                foreach (var descendantChild in FindDescendants<T>(child))
                    yield return descendantChild;
            }
        }

        private void ResetBorderAndOpacity(Button button)
        {
            AnimateBorderThickness(button, new Thickness(0));
            button.Opacity = 0.75;
        }

        private void ApplyBorderAnimation(Button button)
        {
            AnimateBorderThickness(button, new Thickness(1, 0, 0, 0));
            button.Opacity = 1;
        }

        private void AnimateBorderThickness(Button button, Thickness toValue)
        {
            var animation = new ThicknessAnimation
            {
                To = toValue,
                Duration = TimeSpan.FromSeconds(0.25),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            Storyboard.SetTarget(animation, button);
            Storyboard.SetTargetProperty(animation, new PropertyPath(BorderThicknessProperty));
            storyboard.Begin();
        }


        // Script List

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
                        "Open file in new tab?", $"{Strings.Get("AppName")}", MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
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

        private bool isDragging = false;
        private System.Windows.Point lastMousePosition;
        private double currentValue = 1.0;
        private double backgroundOpacityValue = 1.0;
        private double backgroundBlurValue = 1.0;
        private double editorTransparencyValue = 1.0;
        private string currentDragButton = "";

        private void OpacityButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                isDragging = true;
                currentDragButton = ((FrameworkElement)sender).Name;
                lastMousePosition = e.GetPosition(this);
                this.CaptureMouse();
                e.Handled = true;
            }
        }

        private void GlMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                System.Windows.Point currentPosition = e.GetPosition(this);
                double deltaX = currentPosition.X - lastMousePosition.X;

                // Use currentDragButton instead of sender.Name
                switch (currentDragButton)
                {
                    case "WindowOpacityDragDrop":
                        currentValue += deltaX * 0.007;
                        currentValue = Math.Max(0.1, Math.Min(1.0, currentValue));
                        OpacityValueIndicator.Content = $"{currentValue.ToString("F2")}";
                        Shared.mainWindow!.Opacity = currentValue;
                        foreach (TabItem item in TabSystem_.maintabs.Items)
                        {
                            MonacoApi TabInstance = (item.Content as MonacoApi)!;
                            TabInstance.ChangeEditorTransparency((double)currentValue);
                        }
                        break;

                    case "WindowBackOpacityDragDrop":
                        backgroundOpacityValue += deltaX * 0.007;
                        backgroundOpacityValue = Math.Max(0.0, Math.Min(backgroundOpacityValue, 1.0));
                        OpacityBackValueIndicator.Content = $"{backgroundOpacityValue.ToString("F2")}";
                        WindowBackgroundImage.Opacity = backgroundOpacityValue;
                        break;

                    case "WindowBackBlurDragDrop":
                        backgroundBlurValue += deltaX * 0.5;
                        backgroundBlurValue = Math.Max(0.0, Math.Min(backgroundBlurValue, 140.0));
                        OpacityBackBlurValueIndicator.Content = $"{backgroundBlurValue.ToString()}";
                        WindowBackgroundBlur.Radius = backgroundBlurValue;
                        break;

                    case "EditorTransparencySetting":
                        editorTransparencyValue += deltaX * 0.007;
                        editorTransparencyValue = Math.Max(0.1, Math.Min(1.0, editorTransparencyValue));
                        EditorTransparencyValueIndicator.Content = $"{editorTransparencyValue.ToString("F2")}";
                        foreach (TabItem item in TabSystem_.maintabs.Items)
                        {
                            MonacoApi TabInstance = (item.Content as MonacoApi)!;
                            TabInstance.ChangeEditorTransparency((double)editorTransparencyValue);
                        }
                        break;

                    default:
                        break;
                }
                lastMousePosition = currentPosition;
            }
        }

        private void EditorGotFocus(object sender, RoutedEventArgs e)
        {
            GlMouseClick(sender, new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            {
                RoutedEvent = Mouse.MouseDownEvent
            });
        }

        private void GlWndMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging)
            {
                isDragging = false;
                this.ReleaseMouseCapture();

                switch (currentDragButton)
                {
                    case "WindowOpacityDragDrop":
                        Themes.SetColor("WindowOpacity", currentValue.ToString());
                        break;

                    case "WindowBackOpacityDragDrop":
                        Themes.SetColor("BackgroundOpacity", backgroundOpacityValue.ToString());
                        break;

                    case "WindowBackBlurDragDrop":
                        Themes.SetColor("BackgroundBlurRadius", backgroundBlurValue.ToString());
                        break;

                    case "EditorTransparencySetting":
                        Themes.SetColor("EditorTransparency_Override", editorTransparencyValue.ToString());
                        break;

                    default:
                        break;
                }

                currentDragButton = "";
            }
            GlMouseClick(sender, e);
        }

        private void GlMouseClick(object sender, MouseButtonEventArgs e)
        {
            if (Shared.mainWindow!.OwnedWindows.Count != 0)
            {
                foreach (Window ownedWindow in Shared.mainWindow.OwnedWindows)
                {
                    if (!ownedWindow.IsActive)
                    {
                        bool isDialog;
                        try
                        {
                            ownedWindow.DialogResult = false;
                            isDialog = true;
                        }
                        catch (InvalidOperationException)
                        {
                            isDialog = false;
                        }

                        if (!isDialog)
                        {
                            ownedWindow.Hide();
                            ownedWindow.Close();
                        }
                    }
                }
            }
        }

        public void ChangeWndBackgroundImg(object sender, RoutedEventArgs e)
        {
            string? input = InputText.ShowInputDialog("Change Window Background", "Write an image URL below.\n(File locations are supported)", useBrowseButton: true);
            if (!string.IsNullOrWhiteSpace(input))
            {
                Themes.SetColor("BackgroundLocation", input);
                WindowBackgroundImage.Source = input.StartsWith("http")
                    ? new System.Windows.Media.Imaging.BitmapImage(new Uri(input))
                    : new System.Windows.Media.Imaging.BitmapImage(new Uri(input, UriKind.RelativeOrAbsolute));
        }
        }

        public void ColorChanged(object sender,
            RoutedEventArgs e) => HandleColorChange((Button)sender);

        /// <summary>
        /// Handles the color change operation for the specified button.
        /// Opens a color picker dialog to select a new color.
        /// Updates the button's background color and saves the color information to the
        /// application's theme.
        /// </summary>
        /// <param name="button">The button for which the color change is being handled.</param>
        public void HandleColorChange(Button button)
        {

            string buttonTag = button.Tag.ToString()!; // sounds like gutten morgen fr
            Color sigmaColor = Colors.Transparent;
            if (button.Tag.ToString()!.StartsWith("_"))
                sigmaColor = Splash.ParseColor(Themes.GetColor(buttonTag));
            else
                sigmaColor = ConsoleControl.ParseColor(Themes.GetColor(buttonTag)).Color;

            var dialog = new ColorPickerDialog(sigmaColor);
            dialog.ResizeMode = ResizeMode.NoResize;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            dialog.Topmost = true;
            dialog.Owner = Shared.mainWindow;
            var result = dialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                var color = dialog.Color;
                var solidColorBrush = new SolidColorBrush(color);

                if (Application.Current.Resources.Contains(buttonTag))
                    Application.Current.Resources[buttonTag] = buttonTag.StartsWith("_") ? solidColorBrush.Color : solidColorBrush;

                button.Background = solidColorBrush;
                string colorHexString = color.ToString();

                Themes.SetColor(buttonTag!, colorHexString);
            }

            ParseMyTheme();
        }

        /// <summary>
        /// Reads Registry Keys of Colors selected by the user, and then, puts it into a class to be
        /// applied and parsed.
        /// </summary>
        public void ParseMyTheme()
        {
            Application.Current.Resources[PrimaryColor.Tag.ToString()] = ConsoleControl.ParseColor(Themes.GetColor("PrimaryColor"));
            Application.Current.Resources[SecondaryColor.Tag.ToString()] = ConsoleControl.ParseColor(Themes.GetColor("SecondaryColor"));
            Application.Current.Resources[TertiaryColor.Tag.ToString()] = ConsoleControl.ParseColor(Themes.GetColor("TertiaryColor"));
            Application.Current.Resources[Text.Tag.ToString()] = ConsoleControl.ParseColor(Themes.GetColor("Text"));
            Application.Current.Resources[SecondaryText.Tag.ToString()] = ConsoleControl.ParseColor(Themes.GetColor("SecondaryText"));

            Application.Current.Resources["EslScrollbarThumb"] = ConsoleControl.ParseColor(Themes.GetColor("SecondaryText"));

            ThomasShelbyRadialGradient.Color = Splash.ParseColor(Themes.GetColor("_SplashColor1"));
        }

        /// <summary>
        /// Set the background of the Interactuable Buttons that the user uses to match the color in
        /// Registry Had to do it since WPF is not an Immediate Mode GUI
        /// </summary>
        public void ParseMyThemeSelectors()
        {
            SetControlBackground(SSC1, "_SplashColor1");
            SetControlBackground(SSC2, "_SplashColor2");
            SetControlBackground(PrimaryColor, "PrimaryColor");
            SetControlBackground(SecondaryColor, "SecondaryColor");
            SetControlBackground(TertiaryColor, "TertiaryColor");
            SetControlBackground(Text, "Text");
            SetControlBackground(SecondaryText, "SecondaryText");
            // SetControlBackground(null!, "_SplashColor1", ThomasShelbyRadialGradient);
        }

        /// <summary>
        /// Used in <see cref="ParseMyThemeSelectors"/>
        /// </summary>
        /// <param name="control"></param>
        /// <param name="colorKey"></param>
        public void SetControlBackground(FrameworkElement control, string colorKey, GradientStop gStop = null!)
        {
            if (gStop != null)
            {
                gStop.Color = Splash.ParseColor(Themes.GetColor(colorKey));
                return;
            }

            PropertyInfo backgroundProperty = control.GetType().GetProperty("Background")!;
            if (backgroundProperty != null)
            {
                SolidColorBrush brush = (SolidColorBrush)new BrushConverter().ConvertFromString(Themes.GetColor(colorKey))!;
                backgroundProperty.SetValue(control, brush);
                return;
            }

            PropertyInfo colorProperty = control.GetType().GetProperty("Color")!;
            if (colorProperty != null)
            {
                Color color = Splash.ParseColor(Themes.GetColor(colorKey));
                colorProperty.SetValue(control, color);
                return;
            }
        }

        /// <summary>
        /// Resets the Theme Values to the Default ones made by me (Excel).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ResetTheme_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.DialogResult yesNoHi = System.Windows.Forms.MessageBox.Show("Are you sure you want to reset the theme? This will revert all your custom colors to the default ones.", $"{Strings.Get("AppName")} / Reset Theme", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (yesNoHi == DialogResult.Yes)
            {
                ResetTheme();
                ParseMyTheme();
                ParseMyThemeSelectors();

                Application.Current.Resources[PrimaryColor.Tag.ToString()] = ConsoleControl.ParseColor(Themes.GetColor("PrimaryColor"));
                Application.Current.Resources[SecondaryColor.Tag.ToString()] = ConsoleControl.ParseColor(Themes.GetColor("SecondaryColor"));
                Application.Current.Resources[TertiaryColor.Tag.ToString()] = ConsoleControl.ParseColor(Themes.GetColor("TertiaryColor"));
                Application.Current.Resources[Text.Tag.ToString()] = ConsoleControl.ParseColor(Themes.GetColor("Text"));
                Application.Current.Resources[SecondaryText.Tag.ToString()] = ConsoleControl.ParseColor(Themes.GetColor("SecondaryText"));
            }
        }

        public async void ExportTheme_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new System.Windows.Forms.SaveFileDialog
            {
                FileName = $"{Environment.UserName}_li-theme.reg",
                Title = "Save to File | LInjector",
                Filter = "Registry File (*.reg)|*.reg|All files (*.*)|*.*",
            };

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;

                string outputFile = saveFileDialog.FileName;
                string keyPath = $"HKCU\\Software\\{Strings.Get("AppName")}\\Theme";

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "reg.exe",
                    Arguments = $"export \"{keyPath}\" \"{outputFile}\" /y",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process.Start(psi);

                await Logs.Console($"Theme exported to {outputFile}", true);
                System.Windows.MessageBox.Show($"Theme successfully exported!\n\nTo load it, you just have to open the generated file, located in {outputFile}", $"{Strings.Get("AppName")} / Theme Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ResetTheme()
        {
            Dictionary<string, string> defaultThemeValues = new()
            {
                { "_SplashColor1", "#FF460B80" },
                { "_SplashColor2", "#FF570057" },
                { "PrimaryColor", "#FF0F0F0F" },
                { "SecondaryColor", "#FF111111" },
                { "TertiaryColor", "#FF141414" },
                { "Text", "#FFFFFFFF" },
                { "SecondaryText", "#FFD3D3D3" },
                { "WindowOpacity", "1" },
                { "BackgroundLocation", "https://excel.lexploits.top/extra/default_background.jpg" },
                { "BackgroundOpacity", "0" },
                { "BackgroundBlurRadius", "0" },
                { "EditorTransparency_Override", "1" }
            };

            foreach (var theme in defaultThemeValues)
            {
                Themes.SetColor(theme.Key, theme.Value);
            }
            ApplyConfig(null!, null!);
        }
    }
}
