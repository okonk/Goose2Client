using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

namespace Goose2Client
{
    public class HotkeySetting
    {
        public enum SlotType
        {
            Spell = 0,
            Item = 1
        }

        public int SlotNumber;
        public SlotType Type;

        public HotkeySetting(int slotNumber, SlotType type)
        {
            this.SlotNumber = slotNumber;
            this.Type = type;
        }
    }

    public class WindowSettings
    {
        public Vector2 Position;
    }

    public class CharacterSettings
    {
        public HotkeySetting[] Hotkeys;

        public Dictionary<string, WindowSettings> WindowSettings;

        public string MountName;

        private readonly string characterName;

        public CharacterSettings() {}

        public CharacterSettings(string characterName)
        {
            this.characterName = characterName.ToLowerInvariant();

            if (!Load())
                LoadDefaultSettings();

            WindowSettings ??= new();
        }

        private string GetFilePath()
        {
            return Path.Combine(Application.persistentDataPath, $"{characterName}-settings.json");
        }

        public bool Load()
        {
            var filePath = GetFilePath();
            if (!File.Exists(filePath))
                return false;

            var fileContents = File.ReadAllText(filePath);
            var deserialized = JsonConvert.DeserializeObject<CharacterSettings>(fileContents);

            this.Hotkeys = deserialized.Hotkeys;
            this.WindowSettings = deserialized.WindowSettings;
            this.MountName = deserialized.MountName;

            Debug.Log($"Settings loaded: {filePath} {fileContents}");

            return true;
        }

        public void LoadDefaultSettings()
        {
            Hotkeys = new HotkeySetting[10];
            for (int i = 0; i < Hotkeys.Length; i++)
                Hotkeys[i] = new HotkeySetting(-1, HotkeySetting.SlotType.Item);

            WindowSettings = new();
        }

        public void Save()
        {
            var filePath = GetFilePath();
            var fileContents = JsonConvert.SerializeObject(this);

            Debug.Log($"Settings saved: {filePath} {fileContents}");

            File.WriteAllText(filePath, fileContents);
        }

        public WindowSettings GetWindowSettings(string windowName)
        {
            if (WindowSettings.TryGetValue(windowName, out var settings))
                return settings;

            return null;
        }

        public void SetWindowSetting(string windowName, Vector2? position = null)
        {
            var settings = GetWindowSettings(windowName);
            if (settings == null)
            {
                settings = new WindowSettings();
                WindowSettings[windowName] = settings;
            }

            if (position.HasValue)
                settings.Position = position.Value;

            GameManager.Instance.SaveSettingsDelayed();
        }
    }
}