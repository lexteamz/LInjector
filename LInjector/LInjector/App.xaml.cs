using System.Windows;
using System.Windows.Threading;
using LInjector.Classes;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace LInjector
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            // AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            if (!ResourceManager.IsFontInstalled("Segoe Fluent Icons"))
                ResourceManager.InstallFont(await ResourceManager.DownloadFileToTempAsync(Strings.Get("SegoeIconsFontURL"), "Segoe Fluent Icons.ttf"));
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"-/ Unhandled Exception /-" +
                $"\n\n{e.Exception.Message}" +
                $"\n\nStack Trace: {e.Exception.StackTrace}" +
                $"\n\nPlease share this and the replication steps on {Strings.Get("DiscordServerURL")}"

                , "-/ Unhandled Exception /-", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs uE)
        {
            var e = (Exception)uE.ExceptionObject;
            MessageBox.Show($"-/ Fatal Exception /-" +
                $"\n\n{e.Message}" +
                $"\nStack Trace: {e.StackTrace}" +
                $"\n\nPlease share this and the replication steps on {Strings.Get("DiscordServerURL")}" +
                $"\n\nApplication will shutdown after you close this message."

                , "-/ Fatal Exception /-", MessageBoxButton.OK, icon: MessageBoxImage.Error);

            Application.Current.Shutdown();
        }
    }
}