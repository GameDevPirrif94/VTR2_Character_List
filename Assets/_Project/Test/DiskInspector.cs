using System.IO;
using UnityEngine;

public class DiskInspector : MonoBehaviour
{
    void Start()
    {
        Debug.Log("========== DISK INSPECTOR ==========");

        Debug.Log($"Application.dataPath           = {Application.dataPath}");
        Debug.Log($"Application.streamingAssetsPath = {Application.streamingAssetsPath}");
        Debug.Log($"Application.persistentDataPath  = {Application.persistentDataPath}");

        // 1. Проверяем корень Assets
        InspectDir(Application.dataPath, 0, 4);

        // 2. Ищем ВСЕ папки с именем "Resources" и "StreamingAssets" на диске проекта
        Debug.Log("--- Поиск папок Resources / StreamingAssets ---");
        FindDirByName(Application.dataPath, "Resources");
        FindDirByName(Application.dataPath, "StreamingAssets");

        // 3. Проверяем конкретные ожидаемые пути
        CheckFile(Path.Combine(Application.dataPath, "Resources", "Defaults", "default_chronicle.json"));
        CheckFile(Path.Combine(Application.dataPath, "Resources", "Defaults", "aura_table.json"));
        CheckFile(Path.Combine(Application.dataPath, "StreamingAssets", "Defaults", "default_chronicle.json"));
        CheckFile(Path.Combine(Application.dataPath, "StreamingAssets", "Defaults", "aura_table.json"));

        // 4. Ищем где угодно в Assets файл с таким именем
        Debug.Log("--- Поиск default_chronicle.json по всему Assets ---");
        FindFileByName(Application.dataPath, "default_chronicle*");

        Debug.Log("========== END ==========");
    }

    void CheckFile(string path)
    {
        bool exists = File.Exists(path);
        Debug.Log($"[CheckFile] {(exists ? "✅ ЕСТЬ" : "❌ НЕТ")}: {path}");
        if (exists)
        {
            var txt = File.ReadAllText(path);
            Debug.Log($"    Размер: {txt.Length} байт, первые 80 символов: " +
                      txt.Substring(0, Mathf.Min(80, txt.Length)).Replace("\n", " "));
        }
    }

    void InspectDir(string dir, int depth, int maxDepth)
    {
        if (depth > maxDepth || !Directory.Exists(dir)) return;
        string indent = new string(' ', depth * 2);
        Debug.Log($"{indent}[DIR] {Path.GetFileName(dir)}");

        try
        {
            foreach (var f in Directory.GetFiles(dir))
            {
                var info = new FileInfo(f);
                Debug.Log($"{indent}  [FILE] {info.Name} ({info.Length} байт)");
            }
            foreach (var d in Directory.GetDirectories(dir))
                InspectDir(d, depth + 1, maxDepth);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"{indent}  Ошибка чтения: {e.Message}");
        }
    }

    void FindDirByName(string root, string name)
    {
        try
        {
            var dirs = Directory.GetDirectories(root, name, SearchOption.AllDirectories);
            if (dirs.Length == 0)
                Debug.LogWarning($"[FindDir] Папок '{name}' не найдено внутри {root}");
            foreach (var d in dirs)
                Debug.Log($"[FindDir] Найдено: {d}");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[FindDir] Ошибка: {e.Message}");
        }
    }

    void FindFileByName(string root, string pattern)
    {
        try
        {
            var files = Directory.GetFiles(root, pattern, SearchOption.AllDirectories);
            if (files.Length == 0)
                Debug.LogWarning($"[FindFile] Файлов '{pattern}' не найдено внутри {root}");
            foreach (var f in files)
            {
                var info = new FileInfo(f);
                Debug.Log($"[FindFile] Найдено: {f} ({info.Length} байт)");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[FindFile] Ошибка: {e.Message}");
        }
    }
}