using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using VTR.Core.Models;

namespace VTR.Data
{
    /// <summary>
    /// Обёртка над Newtonsoft.Json для сохранения/загрузки моделей.
    /// Все данные кладутся в Application.persistentDataPath/...
    /// </summary>
    public static class JsonSaveSystem
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };

        // --- Корневые папки ---

        public static string RootPath => Application.persistentDataPath;
        public static string ChroniclesPath => Path.Combine(RootPath, "Chronicles");
        public static string LibraryPath => Path.Combine(RootPath, "Library");
        public static string CharactersPath => Path.Combine(RootPath, "Characters");
        public static string PortraitsPath => Path.Combine(RootPath, "Portraits");
        public static string SavesPath => Path.Combine(RootPath, "Saves");

        public static void EnsureFolders()
        {
            Directory.CreateDirectory(ChroniclesPath);
            Directory.CreateDirectory(LibraryPath);
            Directory.CreateDirectory(CharactersPath);
            Directory.CreateDirectory(PortraitsPath);
            Directory.CreateDirectory(SavesPath);
        }

        // --- JSON ---

        public static string Serialize<T>(T obj) => JsonConvert.SerializeObject(obj, Settings);

        public static T Deserialize<T>(string json) where T : new()
            => JsonConvert.DeserializeObject<T>(json, Settings) ?? new T();

        // --- Files ---

        public static void SaveToFile<T>(T obj, string fullPath)
        {
            var dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(fullPath, Serialize(obj));
        }

        public static T LoadFromFile<T>(string fullPath) where T : new()
        {
            if (!File.Exists(fullPath)) return new T();
            return Deserialize<T>(File.ReadAllText(fullPath));
        }

        // --- Хроники ---

        public static string ChronicleFile(string id) => Path.Combine(ChroniclesPath, id + ".json");

        public static void SaveChronicle(Chronicle c) => SaveToFile(c, ChronicleFile(c.Id));
        public static Chronicle LoadChronicle(string id) => LoadFromFile<Chronicle>(ChronicleFile(id));
        public static bool ChronicleExists(string id) => File.Exists(ChronicleFile(id));
        public static void DeleteChronicle(string id)
        {
            var f = ChronicleFile(id);
            if (File.Exists(f)) File.Delete(f);
        }

        // --- Персонажи ---

        public static string CharacterFile(string id) => Path.Combine(CharactersPath, id + ".json");
        public static void SaveCharacter(Character c) => SaveToFile(c, CharacterFile(c.Id));
        public static Character LoadCharacter(string id) => LoadFromFile<Character>(CharacterFile(id));
        public static bool CharacterExists(string id) => File.Exists(CharacterFile(id));

        // --- Портреты ---

        public static string PortraitFile(string characterId, string extension = "png")
            => Path.Combine(PortraitsPath, characterId + "." + extension);

        // --- Генерация Id ---

        public static string NewId(string prefix)
            => prefix + "_" + Guid.NewGuid().ToString("N").Substring(0, 8);
    }
}