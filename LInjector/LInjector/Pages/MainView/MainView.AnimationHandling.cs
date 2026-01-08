using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using Button = System.Windows.Controls.Button;

namespace LInjector.Pages
{
    public partial class MainView
    {
        public void AnimateColor(DropShadowEffect element, System.Windows.Media.Color final, double duration = 1)
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

        private void ApplyBorderAnimation(Button button)
        {
            AnimateBorderThickness(button, new Thickness(1, 0, 0, 0));
            button.Opacity = 1;
        }

        private void ResetBorderAndOpacity(Button button)
        {
            AnimateBorderThickness(button, new Thickness(0));
            button.Opacity = 0.75;
        }
    }
}