using System.IO;
using System.Text.Json;
using Newtonsoft.Json.Linq;

namespace LInjector.Classes
{
    public static class SettingsWrapper
    {
        public static readonly string _configFilePath = Path.Combine(Strings.Get("AppRoot"), "config.json");

        public static Dictionary<string, dynamic> defaultConfig = new Dictionary<string, dynamic>
        {
            {
                "app_init", new
                {
                    value = false,
                    description = "Settings are initialised"
                }
            },
            {
                "auto_attach", new
                {
                    value = false,
                    description = "Auto Attach"
                }
            },
            {
                "splash_screen", new
                {
                    value = true,
                    description = "Show Splash Screen"
                }
            },
            //{
            //    "debug", new
            //    {
            //        value = false,
            //        description = "Enable debug mode for additional logging and diagnostics."
            //    }
            //},
            {
                "top_most", new
                {
                    value = false,
                    description = "Top Most"
                }
            },
            {
                "safe_mode", new
                {
                    value = false,
                    description = "Ah yes, Hamburger's Safe Mode v3"
                }
            },
            {
                "hide_capture", new
                {
                    value = false,
                    description = "Hide from Capture"
                }
            },
            {
                "discord_rpc", new
                {
                    value = false,
                    description = "Discord Rich Presence"
                }
            },
            {
                "save_tabs", new
                {
                    value = false,
                    description = "Save Tabs"
                }
            },
            {
                "show_scriptlist", new
                {
                    value = false,
                    description = "Show the Script List"
                }
            },
            {
                "show_internalconsole", new
                {
                    value = false,
                    description = "Show the LInjector Console"
                }
            },
            {
                "show_minimap", new
                {
                    value = false, // bozo level statement
                    description = "Monaco Editor Minimap"
                }
            },
            {
                "monaco_theme", new
                {
                    value = new[] { "transparent-li-dark", "li-dark", "vs-dark", "vs-light"},
                    description = "Monaco Theme"
                }
            },
            {
                "editor_blurred", new
                {
                    value = false,
                    description = "Blur Monaco Editor"
                }
            },
            {
                "ellipse_topbar", new
                {
                    value = false,
                    description = "Gradient only uses the topbar space"
                }
            }
        };

        public static void Write(string key, dynamic value)
        {
            if (!File.Exists(_configFilePath))
                File.WriteAllText(_configFilePath, "{}");

            string json = File.ReadAllText(_configFilePath);
            JObject config = JObject.Parse(json);

            if (config[key] != null && config[key]!["description"] != null)
            {
                config[key]!["value"] = value;
            }
            else
            {
                config[key] = new JObject
                {
                    ["value"] = value,
                    ["description"] = "No description available."
                };
            }

            string updatedJson = config.ToString(Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(_configFilePath, updatedJson);
        }

        public static dynamic? Read(string key)
        {
            if (!File.Exists(_configFilePath))
                SetDefaultConfig();

            string json = File.ReadAllText(_configFilePath);
            var config = JObject.Parse(json);

            return config.ContainsKey(key) ? config[key]!["value"] : null;
        }

        public static string? ReadDescription(string key)
        {
            if (!File.Exists(_configFilePath))
                SetDefaultConfig();

            string json = File.ReadAllText(_configFilePath);
            var config = JObject.Parse(json);

            return config.ContainsKey(key) ? config[key]!["description"]?.ToString() : null;
        }

        public static void SetDefaultConfig()
        {
            if (!File.Exists(_configFilePath))
            {
                string json = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_configFilePath, json);
            }
            else
            {
                string json = File.ReadAllText(_configFilePath);
                var config = JObject.Parse(json);

                foreach (var key in defaultConfig.Keys)
                {
                    if (!config.ContainsKey(key))
                    {
                        config[key] = JObject.FromObject(defaultConfig[key]);
                    }
                }

                string updatedJson = config.ToString(Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(_configFilePath, updatedJson);
            }
        }
    }
}