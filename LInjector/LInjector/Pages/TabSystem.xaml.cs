using LInjector.Classes;
using LInjector.Pages.Popups;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Drawing.Point;
using UserControl = System.Windows.Controls.UserControl;

namespace LInjector.Pages
{
    public partial class TabSystem : UserControl
    {

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref System.Drawing.Point lpPoint);

        public TabSystem()
        {
            InitializeComponent();
            maintabs.Items.Add(CreateTab("", $"Script {maintabs.Items.Count + 1}.lua"));
        }

        /// <summary>
        /// Returns the current Monaco Editor instance
        /// </summary>
        /// <returns>Current Monaco Editor instance</returns>
        public MonacoApi? CurrentMonaco()
        {
            return maintabs.SelectedContent as MonacoApi;
        }

        /// <summary>
        /// Creates a new tab in the TabSystem control with a separated Monaco Editor instance
        /// </summary>
        /// <param name="text"></param>
        /// <param name="title"></param>
        public void Add_tab_with_text(string text, string? title = null)
        {
            title ??= $"Script {this.maintabs.Items.Count}.lua";

            maintabs.Items.Add(CreateTab(text, title));
        }

        /// <summary>
        /// Changes the current open Monaco Editor tab title
        /// </summary>
        /// <param name="title"></param>
        public void ChangeCurrentTabTitle(string title)
        {
            if (maintabs.SelectedItem is TabItem selectedTab)
            {
                selectedTab.Header = title;
            }
        }

        /// <returns>Current open Monaco Editor instance tab title (name)</returns>
        public Task<string> GetCurrentTabTitle()
        {
            if (maintabs.SelectedItem is TabItem selectedTab)
            {
                return Task.FromResult(selectedTab.Header.ToString())!;
            }

            return Task.FromResult(string.Empty);
        }

        private static readonly string[] sourceArray = [".lua", ".luau", ".txt"];

        public void ButtonTabs(object sender, RoutedEventArgs e)
        {
            switch (((System.Windows.Controls.Button)sender).Name)
            {
                case "AddT":
                    maintabs.Items.Add(CreateTab("", $"Script {maintabs.Items.Count + 1}.lua"));
                    break;
                case "RemoveT":
                    try
                    {
                        if (maintabs.SelectedItem is TabItem tabitem && tabitem.Content is MonacoApi webView)
                        {
                            webView.Dispose();
                        }

                        maintabs.Items.Remove(maintabs.SelectedItem);
                    }
                    catch { }
                    break;
            }
        }

        public static MonacoApi CreateEditor(string Start) => new(Start);

        public TabItem CreateTab(string content, string Title = "Untitled") =>
            new()
            {
                Header = Title,
                Style = TryFindResource("EETABSSSSSS") as Style,
                Foreground = Brushes.White,
                FontSize = 12,
                Content = CreateEditor(content),
                IsSelected = true,
            };


        /// <summary>
        /// Clears the Monaco Editor content and resets the title to its index.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Clear_Editor(object sender, RoutedEventArgs e)
        {
            var x = maintabs.SelectedContent as MonacoApi;
            try
            {
                x!.SetText("");
                this.ChangeCurrentTabTitle($"Script {maintabs.Items.Count}.lua");
            }
            catch { }
        }

        private void Maingrid_PreviewMouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var clickedTabItem = FindAncestor<TabItem>((DependencyObject)sender);

            if (clickedTabItem != null)
            {
                var options = new[]
                {
                    new LIContextMenuStrip.MenuItemOption("Run", "\uF5B0", async (s, ev) =>
                    {
                        //if (ConfigHandler.websocket_mode && Shared.ws.IsRunning)
                        //{
                        //    try
                        //    {
                        //        var cm = clickedTabItem.Content as monaco_api;
                        //        string scriptString = await cm.GetText();

                        //        try
                        //        {
                        //            try
                        //            {
                        //                await Shared.ws.SendMessage(scriptString);
                        //            }
                        //            catch (Exception ex)
                        //            {
                        //                CustomCw.Cw($"LInjector couldn't run the script.\n{ex.Message}\nStack Trace:\n{ex.StackTrace}", false, "error");
                        //            }
                        //        }
                        //        catch (Exception ex)
                        //        {
                        //            MessageBox.Show("LInjector couldn't run the script.", Files.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        //            CustomCw.Cw($"(Module) Exception thrown\n{ex.Message}\nStack Trace:\n{ex.StackTrace}", false, "error");
                        //        }
                        //    }
                        //    catch
                        //    {
                        //        Logs.Console("Unknown error.");
                        //    }
                        //} else
                        //{
                        //    DLLInterface.RunScript(await (clickedTabItem.Content as monaco_api).GetText());
                        //}
                        DLLInterface.RunScript(await (clickedTabItem.Content as MonacoApi)!.GetText());
                    }),

                    new LIContextMenuStrip.MenuItemOption("Rename", "\uE8AC", (s, ev) =>
                    {
                        string? input = InputText.ShowInputDialog("Change Tab Header", "Write the new below.");
                        if (!string.IsNullOrWhiteSpace(input))
                        {
                            if (!sourceArray.Any(ext => input.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                            {
                                input += ".lua";
                            }

                            clickedTabItem.Header = input;
                        }
                    }),

                    new LIContextMenuStrip.MenuItemOption("Toggle Editor Blur", "\uE727", (s, ev) =>
                    {
                        (clickedTabItem.Content as MonacoApi)?.ToggleLocalBlur();
                    }),

                    new LIContextMenuStrip.MenuItemOption("Clear", "\uE75C", (s, ev) =>
                    {
                        (clickedTabItem.Content as MonacoApi)?.SetText("");
                    }),

                    new LIContextMenuStrip.MenuItemOption("Close Tab", "\uE8BB", (s, ev) => {
                        if (clickedTabItem is TabItem tabitem && tabitem.Content is MonacoApi webView)
                        {
                            webView.Dispose();
                        }

                        maintabs.Items.Remove(clickedTabItem);
                    })
                };

                Point defPnt = new();
                GetCursorPos(ref defPnt);

                LIContextMenuStrip.ShowMenu(defPnt, options);
            }
        }


        private static T? FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null && current is not T)
            {
                current = VisualTreeHelper.GetParent(current);
            }
            return current as T;
        }
    }
}
