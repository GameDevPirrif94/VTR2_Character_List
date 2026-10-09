using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using VTR.Core.Models;

namespace VTR.Data
{
    [Serializable]
    public class ContentLibrary
    {
        public string Version = "1.0";

        public List<Discipline> Disciplines = new List<Discipline>();
        public List<Clan> Clans = new List<Clan>();
        public List<Bloodline> Bloodlines = new List<Bloodline>();
        public List<Merit> Merits = new List<Merit>();
        public List<Morality> Moralites = new List<Morality>();
        public List<Addon> Addons = new List<Addon>();
        public List<RollPreset> RollPresets = new List<RollPreset>();

        private static string FilePath => Path.Combine(JsonSaveSystem.LibraryPath, "content_library.json");

        public static ContentLibrary Load()
        {
            JsonSaveSystem.EnsureFolders();
            if (!File.Exists(FilePath))
            {
                var lib = new ContentLibrary();
                lib.Save();
                return lib;
            }
            try
            {
                var json = File.ReadAllText(FilePath);
                var loaded = JsonConvert.DeserializeObject<ContentLibrary>(json, JsonSaveSystem.Settings)
                             ?? new ContentLibrary();
                if (loaded.Disciplines == null) loaded.Disciplines = new List<Discipline>();
                if (loaded.Clans == null) loaded.Clans = new List<Clan>();
                if (loaded.Bloodlines == null) loaded.Bloodlines = new List<Bloodline>();
                if (loaded.Merits == null) loaded.Merits = new List<Merit>();
                if (loaded.Moralites == null) loaded.Moralites = new List<Morality>();
                if (loaded.Addons == null) loaded.Addons = new List<Addon>();
                if (loaded.RollPresets == null) loaded.RollPresets = new List<RollPreset>();
                return loaded;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[ContentLibrary] Ошибка загрузки: {e.Message}");
                return new ContentLibrary();
            }
        }

        public void Save()
        {
            JsonSaveSystem.EnsureFolders();
            File.WriteAllText(FilePath, JsonConvert.SerializeObject(this, JsonSaveSystem.Settings));
        }

        // --- Add/Update ---

        public void AddOrUpdateDiscipline(Discipline d)
        {
            int i = Disciplines.FindIndex(x => x.Id == d.Id);
            if (i >= 0) Disciplines[i] = d; else Disciplines.Add(d);
            Save();
        }

        public void AddOrUpdateClan(Clan c)
        {
            int i = Clans.FindIndex(x => x.Id == c.Id);
            if (i >= 0) Clans[i] = c; else Clans.Add(c);
            Save();
        }

        public void AddOrUpdateBloodline(Bloodline b)
        {
            int i = Bloodlines.FindIndex(x => x.Id == b.Id);
            if (i >= 0) Bloodlines[i] = b; else Bloodlines.Add(b);
            Save();
        }

        public void AddOrUpdateMerit(Merit m)
        {
            int i = Merits.FindIndex(x => x.Id == m.Id);
            if (i >= 0) Merits[i] = m; else Merits.Add(m);
            Save();
        }

        public void AddOrUpdateMorality(Morality m)
        {
            int i = Moralites.FindIndex(x => x.Id == m.Id);
            if (i >= 0) Moralites[i] = m; else Moralites.Add(m);
            Save();
        }

        public void AddOrUpdateAddon(Addon a)
        {
            int i = Addons.FindIndex(x => x.Id == a.Id);
            if (i >= 0) Addons[i] = a; else Addons.Add(a);
            Save();
        }

        public void AddOrUpdateRollPreset(RollPreset p)
        {
            int i = RollPresets.FindIndex(x => x.Id == p.Id);
            if (i >= 0) RollPresets[i] = p; else RollPresets.Add(p);
            Save();
        }
    }
}