using System.Windows;
using System.Windows.Controls;
using LInjector.Classes;
using Button = System.Windows.Controls.Button;
using FontFamily = System.Windows.Media.FontFamily;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Orientation = System.Windows.Controls.Orientation;
using VerticalAlignment = System.Windows.VerticalAlignment;

namespace LInjector.Pages.Popups
{
    public partial class LIContextMenuStrip : Window
    {
        public string? SelectedText { get; private set; }

        public LIContextMenuStrip() => InitializeComponent();

        public class MenuItemOption(string text, string icon, RoutedEventHandler clickHandler)
        {
            public string Text { get; set; } = text;
            public string Icon { get; set; } = icon;
            public RoutedEventHandler ClickHandler { get; set; } = clickHandler;
        }

        public void Add(string text, string icon, RoutedEventHandler clickHandler)
        {
            Button newItem = new()
            {
                Style = TemplateButton.Style,
                Background = TemplateButton.Background,
                Foreground = TemplateButton.Foreground,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = TemplateButton.HorizontalContentAlignment,
                BorderThickness = TemplateButton.BorderThickness,
                OverridesDefaultStyle = TemplateButton.OverridesDefaultStyle,
                Content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = icon,
                            FontFamily = new FontFamily("Segoe MDL2 Assets"),
                            FontSize = 15,
                            Margin = new Thickness(0, 0, 15, 0),
                            Foreground = TemplateButton.Foreground,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Center,
                            TextAlignment = TextAlignment.Left,
                        },

                        new TextBlock
                        {
                            Text = text,
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(0, 0, 0, 0),
                            FontSize = 13,
                            Foreground = TemplateButton.Foreground,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            FontFamily = new FontFamily("Microsoft YaHei UI"),
                        }
                    }
                }
            };

            var stackPanel = (StackPanel)newItem.Content;

            for (int i = stackPanel.Children.Count - 1; i >= 0; i--)
            {
                if (stackPanel.Children[i] == null)
                {
                    stackPanel.Children.RemoveAt(i);
                }
            }

            newItem.Click += (sender, e) =>
            {
                if (sender is Button button && button.Content is StackPanel stackPanel1)
                {
                    var textBlock = stackPanel1.Children.OfType<TextBlock>().FirstOrDefault();
                    if (textBlock != null)
                    {
                        SelectedText = textBlock.Text;
                        this.Close();
                    }
                }

                clickHandler?.Invoke(sender, e);
            };

            ((StackPanel)TemplateButton.Parent).Children.Add(newItem);
        }

        private bool isClosingRequested;

        private void CloseContextMenu()
        {
            if (isClosingRequested) return;

            isClosingRequested = true;
            SelectedText = string.Empty;
            Hide();
            Close();
        }

        public static string ShowMenu(System.Drawing.Point locationInScreen, params MenuItemOption[] options)
        {
            var contextMenu = new LIContextMenuStrip()
            {
                WindowStartupLocation = WindowStartupLocation.Manual,
                Owner = Shared.mainWindow!,
                Left = locationInScreen.X,
                Top = locationInScreen.Y,
            };

            foreach (var option in options)
            {
                contextMenu.Add(option.Text, option.Icon, option.ClickHandler);
            }

            contextMenu.TemplateButton.Visibility = Visibility.Collapsed;

            contextMenu.LostFocus += (_, _) => contextMenu.CloseContextMenu();

            contextMenu.Show();
            contextMenu.Activate();

            return contextMenu.SelectedText ?? string.Empty;
        }
    }
}
