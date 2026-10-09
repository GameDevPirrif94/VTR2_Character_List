using System.IO;
using UnityEngine;
using VTR.Core.Models;

namespace VTR.Data
{
    public static class DefaultsLoader
    {
        public const string DefaultChronicleName = "default_chronicle";
        public const string AuraTableName = "aura_table";

        public static Chronicle LoadDefaultChronicle()
        {
            var text = LoadTextAsset(DefaultChronicleName);
            if (string.IsNullOrEmpty(text))
            {
                Debug.LogError($"[DefaultsLoader] Не найден {DefaultChronicleName}.json. " +
                               $"Убедись, что BootstrapDefaults.EnsureDefaults() был вызван.");
                return new Chronicle();
            }
            return JsonSaveSystem.Deserialize<Chronicle>(text);
        }

        public static AuraTable LoadAuraTable()
        {
            var text = LoadTextAsset(AuraTableName);
            if (string.IsNullOrEmpty(text))
            {
                Debug.LogError($"[DefaultsLoader] Не найден {AuraTableName}.json. " +
                               $"Убедись, что BootstrapDefaults.EnsureDefaults() был вызван.");
                return new AuraTable();
            }
            return JsonSaveSystem.Deserialize<AuraTable>(text);
        }

        public static Chronicle CreateNewChronicle(string name)
        {
            var c = LoadDefaultChronicle();
            c.Id = JsonSaveSystem.NewId("chr");
            c.Name = string.IsNullOrEmpty(name) ? "Новая хроника" : name;
            return c;
        }

        /// <summary>
        /// Ищем в порядке приоритета:
        ///   1) persistentDataPath/Defaults/{name}.json
        ///   2) StreamingAssets/Defaults/{name}.json
        ///   3) Resources/Defaults/{name}
        /// </summary>
        private static string LoadTextAsset(string name)
        {
            // 1. persistentDataPath
            var pdp = Path.Combine(Application.persistentDataPath, "Defaults", name + ".json");
            if (File.Exists(pdp))
            {
                var text = File.ReadAllText(pdp);
                Debug.Log($"[DefaultsLoader] Загружен из persistentDataPath: {pdp} ({text.Length} байт)");
                return text;
            }

            // 2. StreamingAssets
            var sap = Path.Combine(Application.streamingAssetsPath, "Defaults", name + ".json");
            if (File.Exists(sap))
            {
                var text = File.ReadAllText(sap);
                Debug.Log($"[DefaultsLoader] Загружен из StreamingAssets: {sap} ({text.Length} байт)");
                return text;
            }

            // 3. Resources
            var res = Resources.Load<TextAsset>($"Defaults/{name}");
            if (res != null && !string.IsNullOrEmpty(res.text))
            {
                Debug.Log($"[DefaultsLoader] Загружен из Resources: {name} ({res.text.Length} байт)");
                return res.text;
            }

            Debug.LogWarning($"[DefaultsLoader] Не нашёл '{name}' нигде.");
            return null;
        }
    }
}