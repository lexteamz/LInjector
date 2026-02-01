using LInjector.Classes;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Button = System.Windows.Controls.Button;
using Cursors = System.Windows.Input.Cursors;

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

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Section_Settings.Visibility = Visibility.Hidden;
            Section_Editor.Visibility = Visibility.Hidden;
            TabSystem_.Visibility = Visibility.Visible;
            TabSystem_.IsEnabled = true;

            TitleBarLabel.Content = $"{Strings.Get("AppName")}";

            ParseMyTheme();
            ParseConfig();
            ParseMyThemeSelectors();
            Announce.InitVars(the_bocchler, dorito);
            devImageField.Source = new BitmapImage(new Uri(Strings.Get("DeveloperImage")));
            NavigationGridClick(Editor, e);

            if (DateTime.Now.Month == 4 && DateTime.Now.Day == 1)
                foreach (Button button in FindLogicalChildren<Button>(this))
                    button.Click += PlayRandomSound;

            foreach (TextBlock textBlock in FindLogicalChildren<TextBlock>(this))
                if (textBlock.TextDecorations.Any(d => d.Location == TextDecorationLocation.Underline))
                {
                    textBlock.Cursor = Cursors.Hand;
                    textBlock.ToolTip = Strings.Get(textBlock.Uid);
                }

            Shared.mainWindow!.MouseMove += GlMouseMove;
            Shared.mainWindow!.MouseUp += GlWndMouseUp;

            await ScriptContext.EnsureFunctionsFile();
            await ScriptContext.BeginFunctionTick();
            BeginAttachDetection();

        }
    }
}