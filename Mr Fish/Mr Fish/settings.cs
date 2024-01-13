using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Newtonsoft.Json;

namespace MCAutoFish
{
    internal class Settings
    {
        public static readonly string SettingsFilePath = "MCAutoFish.json";
        public static Dictionary<string, object> settings;

        static Settings()
        {
            LoadSettings();
        }

        public static void LoadSettings()
        {
            if (File.Exists(SettingsFilePath))
            {
                string json = File.ReadAllText(SettingsFilePath);
                settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            }
            else
            {
                settings = new Dictionary<string, object>();
            }
        }

        public static void SaveSettings()
        {
            string json = JsonConvert.SerializeObject(settings, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(SettingsFilePath, json);
        }

        public static void Set(string key, object value)
        {
            if (settings.ContainsKey(key))
            {
                settings[key] = value;
            }
            else
            {
                settings.Add(key, value);
            }

            SaveSettings();
        }

        public static T Get<T>(string key)
        {
            return (T)settings[key];

        }
    }
}
