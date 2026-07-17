using LInjector.Classes;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Application = System.Windows.Application;

namespace LInjector.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class Splash : Window
    {
        readonly int RGBSpinSpeed = 4;
        DispatcherTimer RGBTime = new();
        readonly Storyboard StoryBoard = new();

        public static string[] soundEvents = { "windows", "metal", "bamboo" };

        private IEasingFunction Smooth { get; set; } = new QuarticEase
        {
            EasingMode = EasingMode.EaseInOut
        };

        /// <summary>
        /// Moves an object to a desired position
        /// </summary>
        /// <param name="speed">Speed in Miliseconds</param>
        /// <param name="Object">WPF Object</param>
        /// <param name="Get">Initial Position</param>
        /// <param name="Set">Final Position</param>
        public void ObjectShift(Duration speed, DependencyObject Object, Thickness Get, Thickness Set)
        {
            ThicknessAnimation Animation = new()
            {
                From = Get,
                To = Set,
                Duration = speed,
                EasingFunction = Smooth,
            };
            Storyboard.SetTarget(Animation, Object);
            Storyboard.SetTargetProperty(Animation, new PropertyPath(MarginProperty));
            StoryBoard.Children.Add(Animation);
            StoryBoard.Begin();
            StoryBoard.Children.Remove(Animation);
        }

        public Splash()
        {

            string[] arguments = Environment.GetCommandLineArgs();
            foreach (string arg in arguments)
            {
                switch (arg)
                {
                    case "--metalpipe":
                    case "-mp":
                        StartupHandler.PlayStartupSound(soundEvents[1]);
                        break;

                    case "--bamboo":
                    case "-bp":
                        StartupHandler.PlayStartupSound(soundEvents[2]);
                        break;

                    case "--windows":
                    case "-win":
                        StartupHandler.PlayStartupSound(soundEvents[0]);
                        break;
                    case "--debug":
                    case "-dbg":
                        ConsoleManager.Show();
                        break;
                }
            }

            if (DateTime.Now.Month == 4 && DateTime.Now.Day == 1) // April 1st
            {
                int rand = new Random().Next(0, soundEvents.Length);
                string RandomEvent = soundEvents[rand];

                StartupHandler.PlayStartupSound(RandomEvent);
            }

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

            bool needsDefaultValues = defaultThemeValues.Keys.Any(key => !Themes.LookColor(key));
            if (needsDefaultValues)
            {
                foreach (var theme in defaultThemeValues)
                {
                    Themes.SetColor(theme.Key, theme.Value);
                }
            }

            InitializeComponent();

            if (SettingsWrapper.Read("splash_screen") == false)
            {
                this.Hide();
                ShowWindow();
            }

            RPCManager.InitRPC();
        }

        private void DoubleAnimation_Completed(object sender, EventArgs e)
        {
            Storyboard fadeOutStoryboard = new();
            DoubleAnimation fadeOutAnimation = new()
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3)
            };
            fadeOutStoryboard.Children.Add(fadeOutAnimation);
            Storyboard.SetTarget(fadeOutAnimation, this);
            Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath(Window.OpacityProperty));
            fadeOutStoryboard.Completed += OnFadeOutCompleted!;
            fadeOutStoryboard.Begin();
        }

        private void OnFadeOutCompleted(object sender, EventArgs e)
        {
            Hide();
            ShowWindow();
        }

        private static void ShowWindow()
        {
            Shared.mainWindow = new MainWindow();
            Shared.mainWindow.Show();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Shared.SetWindowCaptureProtection(new WindowInteropHelper(this).Handle, (bool)SettingsWrapper.Read("hide_capture"));

            GradientStop1.Color = ParseColor(Themes.GetColor("_SplashColor1"));
            GradientStop2.Color = ParseColor(Themes.GetColor("_SplashColor2"));

            BSG1.Color = ParseColor(Themes.GetColor("_SplashColor1"));
            BSG2.Color = ParseColor(Themes.GetColor("_SplashColor2"));

            RGBTime = new DispatcherTimer(TimeSpan.FromMilliseconds(10), DispatcherPriority.Normal, delegate { rgbRotation.Angle += RGBSpinSpeed; }, dispatcher: Application.Current.Dispatcher);
            RGBTime!.Start();

            ObjectShift(TimeSpan.FromSeconds(1), LInjectorIcon, LInjectorIcon.Margin, new Thickness(0, 0, 0, 0));
            //spritePlayer.LoadSpriteSheet("pack://application:,,,/LInjector;component/Resources/animated_spritesheet.png", 256, 256, 600, 30, 20, 3); /*set the 'animated_spritesheet.png file as Resource */
            //ObjectShift(TimeSpan.FromSeconds(1), spritePlayer, spritePlayer.Margin, new Thickness(0, 0, 0, 0));
        }

        public static System.Windows.Media.Color ParseColor(string srgb)
        {
            if (srgb.Contains("#"))
                srgb = srgb.TrimStart('#');

            if (srgb.Length != 8)
            {
                throw new ArgumentException($"sRGB must be 8 characters, got {srgb} : {srgb.Length}", nameof(srgb));
            }

            byte a = byte.Parse(srgb.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte r = byte.Parse(srgb.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(srgb.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(srgb.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);

            return System.Windows.Media.Color.FromArgb(a, r, g, b);
        }
    }
}