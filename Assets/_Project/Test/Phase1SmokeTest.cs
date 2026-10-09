using System;
using UnityEngine;
using VTR.Core.Models;
using VTR.Core.Rules;
using VTR.Data;

public class Phase1SmokeTest : MonoBehaviour
{
    [Header("Запустить проверку при старте")]
    public bool runOnStart = true;

    [ContextMenu("Run Smoke Test")]
    public void RunSmokeTest()
    {
        Debug.Log("========== PHASE 1 SMOKE TEST START ==========");

        try
        {
            // Гарантируем, что дефолтные JSON есть на диске
            BootstrapDefaults.EnsureDefaults();

            JsonSaveSystem.EnsureFolders();
            Debug.Log($"[1] Папки созданы: {JsonSaveSystem.RootPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[1] Ошибка создания папок: {e}");
            return;
        }

        Chronicle chr = null;
        try
        {
            chr = DefaultsLoader.CreateNewChronicle("Тест");
            Debug.Log($"[2] Хроника: {chr.Name}, Id={chr.Id}, " +
                      $"ранги={chr.Ranks.Count}, атрибуты={chr.Attributes.Count}, " +
                      $"навыки={chr.Skills.Count}, дисциплины={chr.Disciplines.Count}, " +
                      $"кланы={chr.Clans.Count}, морали={chr.Moralites.Count}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[2] Ошибка загрузки дефолтной хроники: {e}");
            return;
        }

        try
        {
            JsonSaveSystem.SaveChronicle(chr);
            var loaded = JsonSaveSystem.LoadChronicle(chr.Id);
            Debug.Log($"[3] Save/Load OK. Загружено: {loaded.Name}, " +
                      $"ранги={loaded.Ranks.Count}, навыки={loaded.Skills.Count}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[3] Ошибка Save/Load: {e}");
        }

        try
        {
            var r = DiceRoller.Roll(7, 0, 3);
            Debug.Log("[4] " + DiceRoller.FormatLog("Интеллект + Оккультизм + Тауматургия", r));

            var ext = DiceRoller.ExtendedRoll(6, 0, 2, 5);
            Debug.Log("[4] " + DiceRoller.FormatExtendedLog("Взлом замка", ext));

            var fate = DiceRoller.Roll(1, -5, 0);
            Debug.Log("[4] " + DiceRoller.FormatLog("Проверка Судьбы", fate));
        }
        catch (Exception e)
        {
            Debug.LogError($"[4] Ошибка дайс-роллера: {e}");
        }

        try
        {
            int xp1 = XpCalculator.AttributeCost(chr.XpSystem, 3, favored: false);
            int xp2 = XpCalculator.AttributeCost(chr.XpSystem, 3, favored: true);
            int xp3 = XpCalculator.SkillCost(chr.XpSystem, 2, favored: false);
            Debug.Log($"[5] XP: атрибут 3->4 = {xp1}, любимый = {xp2}, навык 2->3 = {xp3}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[5] Ошибка XP-калькулятора: {e}");
        }

        try
        {
            var aura = DefaultsLoader.LoadAuraTable();
            int a7 = AuraCalculator.Calculate(aura, 7, AuraType.Regular);
            int a10 = AuraCalculator.Calculate(aura, 10, AuraType.Strong);
            int a1 = AuraCalculator.Calculate(aura, 1, AuraType.Weak);
            Debug.Log($"[6] Аура: мораль 7/обычная = {a7}, мораль 10/сильная = {a10}, мораль 1/слабая = {a1}");

            int wp = AuraCalculator.CalculateMaxWillpower(3, 3);
            Debug.Log($"[6] Сила воли (Самоконтроль 3 + Решительность 3) = {wp}");

            int blood = AuraCalculator.CalculateMaxBlood(chr, 2);
            Debug.Log($"[6] Запас крови при густоте 2 = {blood}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[6] Ошибка калькуляторов: {e}");
        }

        try
        {
            var skill = chr.FindSkill("drive");
            if (skill != null)
            {
                chr.SelectedTimeFrame = "DA";
                Debug.Log($"[7] Навык drive в XX: '{skill.Name}', в DA: '{chr.GetSkillName(skill)}'");
                chr.SelectedTimeFrame = "MN";
            }
            else
            {
                Debug.LogWarning("[7] Навык 'drive' не найден в хронике.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[7] Ошибка временных рамок: {e}");
        }

        Debug.Log("========== PHASE 1 SMOKE TEST END ==========");
    }

    private void Start()
    {
        if (runOnStart) RunSmokeTest();
    }
}