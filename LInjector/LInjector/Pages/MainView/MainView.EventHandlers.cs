using LInjector.Classes;
using LInjector.Windows;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Application = System.Windows.Application;
using Cursors = System.Windows.Input.Cursors;

namespace LInjector.Pages
{
    public partial class MainView
    {
        DispatcherTimer TitleTimer = new DispatcherTimer();
        DispatcherTimer bozoTimer = new DispatcherTimer();
        DispatcherTimer Discordtimer = new DispatcherTimer(DispatcherPriority.Background);

        private bool isDragging = false;
        private System.Windows.Point lastMousePosition;
        private double currentValue = 1.0;
        private double backgroundOpacityValue = 1.0;
        private double backgroundBlurValue = 1.0;
        private double editorTransparencyValue = 1.0;
        private string currentDragButton = "";

        private void PlayRandomSound(object sender, RoutedEventArgs e)
        {
            int rand = new Random().Next(1, Splash.soundEvents.Length);
            string RandomEvent = Splash.soundEvents[rand];

            StartupHandler.PlayStartupSound(RandomEvent);
        }

        public void BeginDiscordRPC()
        {
            Discordtimer.Interval = TimeSpan.FromSeconds(5);
            Discordtimer.Tick += DiscordRPCTick!;
            Discordtimer.Start();
        }

        private void Hyperlink_PreviewDown(object sender, MouseButtonEventArgs e) => Shared.OpenURL(Strings.Get((sender as FrameworkElement)!.Uid));

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

        public void bozoTick(object sender, EventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            bool isAttached = DLLInterface.IsAttached() && !(bool)SettingsWrapper.Read("attached_glow_disabled");

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
                Mouse.OverrideCursor = Cursors.ScrollWE;
                System.Windows.Point currentPosition = e.GetPosition(this);
                double deltaX = currentPosition.X - lastMousePosition.X;

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
                Mouse.OverrideCursor = null;

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

        public void OnCloseFadeoutCompleted(object sender, EventArgs e)
        {
            Shared.mainWindow!.Close();
            Application.Current.Shutdown();

            try
            {
                string? webViewCacheDir = TabSystem_.CurrentMonaco()!.CoreWebView2.Environment.UserDataFolder;
                var webViewProcessId = Convert.ToInt32(TabSystem_.CurrentMonaco()!.CoreWebView2.BrowserProcessId);
                var webViewProcess = Process.GetProcessById(webViewProcessId);

                TabSystem_.CurrentMonaco()!.Dispose();
                webViewProcess.WaitForExit(3000);

                Directory.Delete(webViewCacheDir, true);
            }
            catch { }
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

                    var fadeOutAnimation = new System.Windows.Media.Animation.DoubleAnimation { From = 1, To = 0, Duration = TimeSpan.FromSeconds(0.1) };

                    System.Windows.Media.Animation.Storyboard fadeOutStoryboard = new();
                    fadeOutStoryboard.Children.Add(fadeOutAnimation);
                    System.Windows.Media.Animation.Storyboard.SetTarget(fadeOutAnimation, this);
                    System.Windows.Media.Animation.Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath(OpacityProperty));
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
            }
        }
    }
}