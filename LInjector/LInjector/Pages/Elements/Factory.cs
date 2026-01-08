using LInjector.Classes;
using Newtonsoft.Json.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Application = System.Windows.Application;
using Brushes = System.Windows.Media.Brushes;
using CheckBox = System.Windows.Controls.CheckBox;
using ComboBox = System.Windows.Controls.ComboBox;
using FlowDirection = System.Windows.FlowDirection;
using Label = System.Windows.Controls.Label;
using Rectangle = System.Windows.Shapes.Rectangle;
using SystemFonts = System.Windows.SystemFonts;
using TextBox = System.Windows.Controls.TextBox;

namespace LInjector.Pages.Elements
{
    public class Factory
    {
        private readonly StackPanel _container;

        public Factory(StackPanel container)
        {
            _container = container;
        }

        public void CreateToggleOption(Delegate callback, object defaultValue, string key, string toolTipContent = "")
        {
            var grid = CreateGridWithConnectingLine();
            var label = CreateLabel(SettingsWrapper.ReadDescription(key)!);
            var toggle = new ToggleButton { Margin = new Thickness(0, 0, 10, 0) };
            if (!string.IsNullOrEmpty(toolTipContent))
                grid.ToolTip = toolTipContent;

            if (defaultValue is bool)
                toggle.IsChecked = (bool)defaultValue;

            toggle.Checked += (s, e) =>
            {
                InvokeCallback(callback, true);
                SettingsWrapper.Write(key, true);
            };
            toggle.Unchecked += (s, e) =>
            {
                InvokeCallback(callback, false);
                SettingsWrapper.Write(key, false);
            };

            toggle.IsEnabled = !key.Contains("app_init");

            Grid.SetColumn(label, 0);
            Grid.SetColumn(toggle, 2);

            grid.Children.Add(label);
            grid.Children.Add(toggle);

            _container.Children.Add(grid);
        }

        public void CreateCheckboxOption(Delegate callback, object defaultValue, string key, string toolTipContent = "")
        {
            var grid = CreateGridWithConnectingLine();
            var label = CreateLabel(SettingsWrapper.ReadDescription(key)!);
            var checkbox = new CheckBox { Margin = new Thickness(0, 0, 15, 0) };
            if (!string.IsNullOrEmpty(toolTipContent))
                grid.ToolTip = toolTipContent;

            if (defaultValue is bool)
                checkbox.IsChecked = (bool)defaultValue;

            checkbox.Checked += (s, e) =>
            {
                InvokeCallback(callback, true);
                SettingsWrapper.Write(key, true);
            };
            checkbox.Unchecked += (s, e) =>
            {
                InvokeCallback(callback, false);
                SettingsWrapper.Write(key, false);
            };

            checkbox.Click += Shared.mainView!.ApplyConfig;

            checkbox.IsEnabled = !key.Contains("app_init");

            Grid.SetColumn(label, 0);
            Grid.SetColumn(checkbox, 2);

            grid.Children.Add(label);
            grid.Children.Add(checkbox);
            _container.Children.Add(grid);
        }

        private double MeasureTextWidth(string text)
        {
            var formattedText = new FormattedText(
                text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(SystemFonts.MessageFontFamily, SystemFonts.MessageFontStyle, SystemFonts.MessageFontWeight, FontStretches.Normal),
                12, // Font size
                Brushes.Black,
                VisualTreeHelper.GetDpi(Application.Current.MainWindow).PixelsPerDip);

            return formattedText.Width;
        }

        public void CreateDropdownOption(Delegate callback, object options, object defaultValue, string key, string optionToWrite, string toolTipContent = "")
        {
            var grid = CreateGridWithConnectingLine();
            var label = CreateLabel(SettingsWrapper.ReadDescription(key)!);

            double maxWidth = 50;
            if (options is Array optionArray)
            {
                foreach (var option in optionArray)
                {
                    double textWidth = MeasureTextWidth(option.ToString()!);
                    maxWidth = Math.Max(maxWidth, textWidth);
                }
            }

            var comboBox = new ComboBox
            {
                Background = System.Windows.Media.Brushes.Transparent,
                Margin = new Thickness(0, 0, 20, 0),
                Width = maxWidth + 20,
                BorderThickness = new Thickness(0),
            };

            comboBox.SetResourceReference(ComboBox.ForegroundProperty, "Text");

            if (options is Array optionArray2)
            {
                foreach (var option in optionArray2)
                {
                    comboBox.Items.Add(new ComboBoxItem { Content = option });
                }
            }

            if (defaultValue != null)
            {
                comboBox.SelectedIndex = Array.IndexOf((Array)options, defaultValue);
            }

            if (!string.IsNullOrEmpty(toolTipContent))
                grid.ToolTip = toolTipContent;

            comboBox.DropDownClosed += (s, e) =>
            {
                string selection = comboBox.Text;

                if (options is string[])
                {
                    var array = options as string[];
                    if (array!.Contains(selection))
                    {
                        var _array = array!.ToList();
                        _array.Remove(selection);
                        _array.Insert(0, selection);

                        options = _array.ToArray();

                        JArray jerarquy = new JArray(_array.ToArray());
                        SettingsWrapper.Write(optionToWrite, jerarquy);
                    }
                }

                InvokeCallback(callback, options);
            };

            comboBox.Text = (options as string[])![0];

            Grid.SetColumn(label, 0);
            Grid.SetColumn(comboBox, 2);

            grid.Children.Add(label);
            grid.Children.Add(comboBox);


            _container.Children.Add(grid);
        }

        public void CreateTextFieldOption(Delegate callback, object defaultValue, bool isNumericOnly = false, string key = "", string toolTipContent = "")
        {
            var grid = CreateGridWithConnectingLine();
            var label = CreateLabel(SettingsWrapper.ReadDescription(key)!);
            var textBox = new TextBox
            {
                Width = 150,
                Margin = new Thickness(0, 0, 20, 0),
            };

            if (defaultValue != null)
                textBox.Text = defaultValue.ToString();

            if (isNumericOnly)
            {
                textBox.PreviewTextInput += (s, e) =>
                {
                    var regex = new System.Text.RegularExpressions.Regex("[^0-9]+");
                    e.Handled = regex.IsMatch(e.Text);
                };
            }

            textBox.TextChanged += (s, e) =>
            {
                InvokeCallback(callback, textBox.Text!);

                object valueToWrite = textBox.Text!;

                if (isNumericOnly)
                {
                    if (int.TryParse(textBox.Text, out int intValue))
                    {
                        valueToWrite = intValue;
                    }
                    else if (float.TryParse(textBox.Text, out float floatValue))
                    {
                        valueToWrite = floatValue;
                    }
                    else if (double.TryParse(textBox.Text, out double doubleValue))
                    {
                        valueToWrite = doubleValue;
                    }
                }

                Shared.mainView!.ApplyConfig(null!, null!);
                SettingsWrapper.Write(key, valueToWrite);
            };

            Grid.SetColumn(label, 0);
            Grid.SetColumn(textBox, 2);

            grid.Children.Add(label);
            grid.Children.Add(textBox);
            _container.Children.Add(grid);
        }

        private void InvokeCallback(Delegate callback, object value)
        {
            callback.DynamicInvoke(value);
        }

        private Grid CreateGridWithConnectingLine()
        {
            var grid = new Grid { Margin = new Thickness(10, 0, -10, 0) };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

            var connectingLine = new Rectangle
            {
                Height = 0.5,
                Opacity = 0.3,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 3, 5, 0),
                StrokeDashArray = new DoubleCollection { 2, 2 }
            };

            connectingLine.SetResourceReference(Rectangle.FillProperty, "SecondaryText");

            Grid.SetColumn(connectingLine, 1);
            grid.Children.Add(connectingLine);

            return grid;
        }

        private Grid CreateGrid()
        {
            return new Grid { Margin = new Thickness(10, 0, 10, 0) };
        }

        private Label CreateLabel(string text)
        {
            var label = new Label
            {
                Content = text,
                FontSize = 12,
            };
            label.SetResourceReference(Label.ForegroundProperty, "Text");
            return label;
        }
    }
}