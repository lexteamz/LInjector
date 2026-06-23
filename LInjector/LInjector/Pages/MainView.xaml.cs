using LInjector.Classes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        public ICommand? SaveCommand { get; }
        public ICommand? SaveAsCommand { get; }
        public ICommand? CloseTabCommand { get; }
        public ICommand? NewTabCommand { get; }
        public ICommand? OpenFileCommand { get; }

        public MainView()
        {
            InitializeComponent();

            SaveCommand = new RelayCommand(_ => Show_SaveToFileDialog());
            SaveAsCommand = new RelayCommand(_ => Show_SaveToFileDialog());
            CloseTabCommand = new RelayCommand(_ =>
            {
                if (TabSystem_.maintabs.SelectedItem is TabItem tabitem && tabitem.Content is MonacoApi webView)
                {
                    webView.Dispose();
                    TabSystem_.maintabs.Items.Remove(tabitem);
                }
            });
            NewTabCommand = new RelayCommand(_ => TabSystem_.maintabs.Items.Add(TabSystem_.CreateTab("", $"Script {TabSystem_.maintabs.Items.Count + 1}.lua")));
            OpenFileCommand = new RelayCommand(_ => Show_OpenFileDialog());

            DataContext = this;
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