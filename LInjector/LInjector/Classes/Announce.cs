using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Label = System.Windows.Controls.Label;

namespace LInjector.Classes
{
    public static class Announce
    {

        private static Label? label;
        private static FrameworkElement? container;
        private static bool isBusy;

        /// <summary>
        /// Shows a notification that popups in the LInjector contextual menu.
        /// </summary>
        ///
        public static async Task Do(string message)
        {

            if (isBusy)
            {
                while (isBusy)
                {
                    await Task.Delay(100);
                }
            }

            if (label == null)
            {
                return;
            }

            isBusy = true;

            double originalBottomMargin = container!.Margin.Bottom;

            await AnimateMarginChange(container, new Thickness(0, 0, 0, 0), TimeSpan.FromSeconds(0.5));

            label.Content = "";

            foreach (var character in message)
            {
                label.Content += character.ToString();
                await Task.Delay(15);
            }

            await Task.Delay(3000);
            await AnimateMarginChange(container, new Thickness(0, 0, 0, originalBottomMargin), TimeSpan.FromSeconds(0.8));

            for (int i = label.Content.ToString()!.Length; i > 0; i--)
            {
                label.Content = label.Content.ToString()!.Remove(i - 1);
                await Task.Delay(10);
            }

            await AnimateMarginChange(container, new Thickness(0, 0, 0, originalBottomMargin), TimeSpan.FromSeconds(0.5));

            isBusy = false;
        }

        public static void InitVars(FrameworkElement blud, Label bozo)
        {
            // I'm to lazy to initialise these in MainView.cs
            label = bozo;
            container = blud;

            if (label == null || container == null)
            {
                container = Shared.mainView!.the_bocchler;
                label = Shared.mainView!.dorito;
            }

            label.Content = "";
        }

        private static async Task AnimateMarginChange(FrameworkElement element, Thickness newMargin, TimeSpan duration)
        {
            var storyboard = new Storyboard();

            var marginAnimation = new ThicknessAnimation
            {
                From = element.Margin,
                To = newMargin,
                Duration = new Duration(duration)
            };

            Storyboard.SetTarget(marginAnimation, element);
            Storyboard.SetTargetProperty(marginAnimation, new PropertyPath("Margin"));

            storyboard.Children.Add(marginAnimation);
            storyboard.Begin();

            await Task.Delay(duration);
        }


        public static async Task AnimateObjectContentAsync(Label Instance, string Message)
        {
            for (int i = Instance.Content.ToString()!.Length; i > 0; i--)
            {
                Instance.Content = Instance.Content.ToString()!.Remove(i - 1);
                await Task.Delay(20);
            }

            Instance.Content = string.Empty;

            foreach (var character in Message)
            {
                Instance.Content += character.ToString();
                await Task.Delay(30);
            }
        }

    }
}