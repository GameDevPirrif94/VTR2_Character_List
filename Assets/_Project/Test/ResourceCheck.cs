using UnityEngine;

public class ResourceCheck : MonoBehaviour
{
    void Start()
    {
        // Пробуем разные варианты имени, чтобы понять, где что лежит
        TryLoad("Defaults/default_chronicle");
        TryLoad("Defaults/default_chronicle.json");
        TryLoad("Defaults/default_chronicle.txt");
        TryLoad("default_chronicle");

        // Показываем ВСЕ TextAsset, которые Unity видит в Resources
        var all = Resources.LoadAll<TextAsset>("");
        Debug.Log($"[ResourceCheck] Всего TextAsset в Resources: {all.Length}");
        foreach (var ta in all)
            Debug.Log($"[ResourceCheck]   → {ta.name} ({ta.text.Length} байт)");
    }

    void TryLoad(string path)
    {
        var ta = Resources.Load<TextAsset>(path);
        Debug.Log($"[ResourceCheck] '{path}' → {(ta != null ? $"OK, {ta.text.Length} байт" : "null")}");
    }
}