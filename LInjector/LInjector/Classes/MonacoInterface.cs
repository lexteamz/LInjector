using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;

namespace LInjector.Classes
{
    public class MonacoApi : WebView2
    {
        // Thanks to parex for this awesome base!
        // I will not touch this

        public bool IsDOMLoaded { get; set; } = false;
        private readonly string ToSetText;
        private string? LatestRecievedText;
        public bool IsMinimapEnabled { get; set; }
        public string LatestRecievedText1 { get => LatestRecievedText!; set => LatestRecievedText = value; }

        /// <summary>
        /// Event for when the editor is fully loaded.
        /// </summary>
        public event EventHandler? EditorReady;

        public MonacoApi(string Text)
        {
            Source = new Uri("https://lexploits.top/monaco");

            CoreWebView2InitializationCompleted += MonacoApiCoreWebView2InitializationCompleted!;
            ToSetText = Text;
            DefaultBackgroundColor = Color.Transparent;
        }

        protected virtual void OnEditorReady()
        {
            EditorReady?.Invoke(this, new EventArgs());
            SetText(ToSetText);
            SetTheme($"\"{(SettingsWrapper.Read("monaco_theme")!.ToObject<string[]>())[0]}\"");
            double wndOpacity = double.Parse(Themes.GetColor("WindowOpacity")!);
            double editorOpacity = double.Parse(Themes.GetColor("EditorTransparency_Override"));
            ChangeEditorTransparency(wndOpacity != 1.0 ? wndOpacity : editorOpacity);
            if (SettingsWrapper.Read("editor_blurred") == true) EnableBlur(); else DisableBlur();
        }

        public void MonacoApiCoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            CoreWebView2.DOMContentLoaded += CoreWebView2_DOMContentLoaded!;
            CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived!;

            CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            CoreWebView2.Settings.AreDevToolsEnabled = false;
            CoreWebView2.Settings.IsZoomControlEnabled = false;
            CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
        }

        private void CoreWebView2_WebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e) => LatestRecievedText = e.TryGetWebMessageAsString();

        private async void CoreWebView2_DOMContentLoaded(object sender, Microsoft.Web.WebView2.Core.CoreWebView2DOMContentLoadedEventArgs e)
        {
            await Task.Delay(1000);
            IsDOMLoaded = true;
            OnEditorReady();
        }
        public async Task<string> GetText()
        {
            if (IsDOMLoaded)
            {
                var script = "monaco.editor.getModels()[0].getValue()";
                var result = await CoreWebView2.ExecuteScriptAsync(script);
                var text = JsonConvert.DeserializeObject<string>(result);

                return text!;
            }
            return string.Empty;
        }

        public async void SetText(string text)
        {
            text = text.Replace("\\", "\\\\");
            await CoreWebView2.ExecuteScriptAsync("editor.setValue('');");
            await CoreWebView2.ExecuteScriptAsync($"editor.setValue(`{text.Replace("`", "\\`")}`)");
        }

        public void AddIntellisense(string label, Types type, string description, string insert)
        {
            if (IsDOMLoaded)
            {
                string selectedType = type.ToString();
                if (type == Types.None)
                    selectedType = "";
                ExecuteScriptAsync($"AddIntellisense({label}, {selectedType}, {description}, {insert});");
            }
        }

        public string GetTheme()
        {
            if (IsDOMLoaded)
            {
                var script = $"GetTheme();";
                string result = CoreWebView2.ExecuteScriptAsync(script).Result;

                return result;
            }

            return string.Empty;
        }

        public void EnableBlur()
        {
            if (IsDOMLoaded)
                ExecuteScriptAsync($"ToggleBlur(true)");
        }

        public void DisableBlur()
        {
            if (IsDOMLoaded)
                ExecuteScriptAsync($"ToggleBlur(false)");
        }

        public void ToggleLocalBlur()
        {
            if (IsDOMLoaded)
                ExecuteScriptAsync($"EnableDisableBlur()");
        }

        public void SetTheme(string id)
        {
            if (IsDOMLoaded)
                ExecuteScriptAsync($"SetTheme({id});");
        }

        public void Refresh()
        {
            if (IsDOMLoaded)
                ExecuteScriptAsync($"Refresh();");
        }

        public void EnableMinimap()
        {
            if (IsDOMLoaded)
                ExecuteScriptAsync("SwitchMinimap(true);");
            IsMinimapEnabled = true;
        }

        public void DisableMinimap()
        {
            if (IsDOMLoaded)
                ExecuteScriptAsync("SwitchMinimap(false);");
            IsMinimapEnabled = false;
        }

        public void EnableAutocomplete()
        {
            if (IsDOMLoaded)
                ExecuteScriptAsync("EnableAutoComplete();");
        }

        public void DisableAutocomplete()
        {
            if (IsDOMLoaded)
                ExecuteScriptAsync("DisableAutoComplete();");
        }

        public void ChangeEditorTransparency(double opacity)
        {
            if (IsDOMLoaded)
                ExecuteScriptAsync($"ChangeEditorTransparency({opacity});");
        }
    }

    public enum Types
    {
        Class,
        Color,
        Constructor,
        Enum,
        Field,
        File,
        Folder,
        Function,
        Interface,
        Keyword,
        Method,
        Module,
        Property,
        Reference,
        Snippet,
        Text,
        Unit,
        Value,
        Variable,
        None
    }
}