using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Goose2Client
{
    [Serializable]
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

    [Serializable]
    public class CharacterSettings
    {
        public HotkeySetting[] Hotkeys;

        public string MountName;

        private readonly string characterName;

        public CharacterSettings(string characterName)
        {
            this.characterName = characterName.ToLowerInvariant();

            if (!Load())
                LoadDefaultSettings();
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
            JsonUtility.FromJsonOverwrite(fileContents, this);

            Debug.Log($"Settings loaded: {filePath} {fileContents}");

            return true;
        }

        public void LoadDefaultSettings()
        {
            Hotkeys = new HotkeySetting[10];
            for (int i = 0; i < Hotkeys.Length; i++)
                Hotkeys[i] = new HotkeySetting(-1, HotkeySetting.SlotType.Item);
        }

        public void Save()
        {
            var filePath = GetFilePath();
            var fileContents = JsonUtility.ToJson(this);

            Debug.Log($"Settings saved: {filePath} {fileContents}");

            File.WriteAllText(filePath, fileContents);
        }
    }
}