using LInjector.Classes;
using LInjector.Pages.Elements;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace LInjector.Pages
{
    public partial class MainView
    {
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
            Shared.SetWindowCaptureProtection(new System.Windows.Interop.WindowInteropHelper(Shared.mainWindow!).Handle, (bool)SettingsWrapper.Read("hide_capture"));

            // Top Most
            Shared.mainWindow!.Topmost = (bool)(SettingsWrapper.Read("top_most"));

            // Show Script List
            Shared.mainView!.ScriptListDimensions.Width = ((bool)SettingsWrapper.Read("show_scriptlist")) ? new System.Windows.GridLength(120, System.Windows.GridUnitType.Star) : new System.Windows.GridLength(0, System.Windows.GridUnitType.Pixel);

            // Show Logs
            bool showConsole = (bool)SettingsWrapper.Read("show_internalconsole");

            // Configure console dimensions
            Shared.mainView.LInjectorConsoleDimensions.Height = new System.Windows.GridLength(
                showConsole ? 140 : 0,
                showConsole ? System.Windows.GridUnitType.Star : System.Windows.GridUnitType.Pixel
            );

            // Configure editor border
            Shared.mainView.EditorBorder.BorderThickness = new System.Windows.Thickness(
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
    }
}