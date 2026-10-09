using System.IO;
using UnityEngine;

namespace VTR.Data
{
    /// <summary>
    /// Пишет дефолтные JSON-файлы на диск при первом запуске,
    /// если их там ещё нет. Пишет в persistentDataPath/Defaults.
    /// </summary>
    public static class BootstrapDefaults
    {
        public static string DefaultsDir =>
            Path.Combine(Application.persistentDataPath, "Defaults");

        public static string DefaultChroniclePath =>
            Path.Combine(DefaultsDir, "default_chronicle.json");

        public static string AuraTablePath =>
            Path.Combine(DefaultsDir, "aura_table.json");

        /// <summary>
        /// Создаёт файлы, если их нет. Если есть — не трогает.
        /// </summary>
        public static void EnsureDefaults(bool force = false)
        {
            Directory.CreateDirectory(DefaultsDir);

            WriteIfMissing(DefaultChroniclePath, DefaultContent.DefaultChronicleJson, force);
            WriteIfMissing(AuraTablePath, DefaultContent.AuraTableJson, force);

            Debug.Log($"[BootstrapDefaults] Файлы по пути: {DefaultsDir}");
        }

        private static void WriteIfMissing(string path, string content, bool force)
        {
            bool exists = File.Exists(path);
            if (exists && !force)
            {
                Debug.Log($"[BootstrapDefaults] Уже есть: {Path.GetFileName(path)} ({new FileInfo(path).Length} байт)");
                return;
            }
            File.WriteAllText(path, content);
            Debug.Log($"[BootstrapDefaults] Записан: {Path.GetFileName(path)} ({content.Length} байт)");
        }

        /// <summary>Удаляет файлы, чтобы при следующем запуске они создались заново.</summary>
        public static void ResetDefaults()
        {
            if (File.Exists(DefaultChroniclePath)) File.Delete(DefaultChroniclePath);
            if (File.Exists(AuraTablePath)) File.Delete(AuraTablePath);
            Debug.Log("[BootstrapDefaults] Файлы удалены. Следующий запуск пересоздаст их.");
        }
    }
}