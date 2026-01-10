using Dsafa.WpfColorPicker;
using LInjector.Classes;
using LInjector.Pages.Popups;
using LInjector.Windows;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using Color = System.Windows.Media.Color;

namespace LInjector.Pages
{
    public partial class MainView
    {
        public void ChangeWndBackgroundImg(object sender, RoutedEventArgs e)
        {
            string? input = InputText.ShowInputDialog("Change Window Background", "Write an image URL below.\n(File locations are supported)", useBrowseButton: true);
            if (!string.IsNullOrWhiteSpace(input))
            {
                Themes.SetColor("BackgroundLocation", input);
                WindowBackgroundImage.Source = input.StartsWith("http")
                    ? new BitmapImage(new Uri(input))
                    : new BitmapImage(new Uri(input, UriKind.RelativeOrAbsolute));
            }
        }

        public void ColorChanged(object sender, RoutedEventArgs e) => HandleColorChange((Button)sender);

        public void HandleColorChange(Button button)
        {
            string buttonTag = button.Tag.ToString()!;
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

        public void ParseMyThemeSelectors()
        {
            SetControlBackground(SSC1, "_SplashColor1");
            SetControlBackground(SSC2, "_SplashColor2");
            SetControlBackground(PrimaryColor, "PrimaryColor");
            SetControlBackground(SecondaryColor, "SecondaryColor");
            SetControlBackground(TertiaryColor, "TertiaryColor");
            SetControlBackground(Text, "Text");
            SetControlBackground(SecondaryText, "SecondaryText");
        }

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

        public void ResetTheme_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.DialogResult yesNoHi = System.Windows.Forms.MessageBox.Show("Are you sure you want to reset the theme? This will revert all your custom colors to the default ones.", $"{Strings.Get("AppName")} / Reset Theme", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Warning, System.Windows.Forms.MessageBoxDefaultButton.Button2);
            if (yesNoHi == System.Windows.Forms.DialogResult.Yes)
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

                _ = Logs.Console($"Theme exported to {outputFile}", true);
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