namespace VTR.Data
{
    /// <summary>
    /// Хардкод только для первого заполнения диска.
    /// После создания файлов — править их в JSON, а не здесь.
    /// </summary>
    public static class DefaultContent
    {
        public const string DefaultChronicleJson = @"
{
  ""Version"": ""1.0"",
  ""Id"": """",
  ""Name"": ""Новая хроника"",
  ""Description"": """",
  ""TimeFrames"": [""DA"", ""XX"", ""MN""],
  ""SelectedTimeFrame"": ""MN"",
  ""MaxPlayers"": 5,

  ""Natures"": [
    ""Архитектор"", ""Бонвиван"", ""Браво"", ""Заботливый"", ""Праздный"",
    ""Ребёнок"", ""Соперник"", ""Конформист"", ""Интриган"", ""Ворчун"",
    ""Девиант"", ""Режиссёр"", ""Загадка"", ""Глаз Бури"", ""Фанатик"",
    ""Галант"", ""Гуру"", ""Идеалист"", ""Судья"", ""Одиночка"",
    ""Мученик"", ""Мазохист"", ""Монстр"", ""Педагог"", ""Кающийся"",
    ""Перфекционист"", ""Бунтарь"", ""Плут"", ""Выживший"", ""Искатель приключений"",
    ""Традиционалист"", ""Трикстер"", ""Визионер""
  ],
  ""Masks"": [
    ""Архитектор"", ""Бонвиван"", ""Браво"", ""Заботливый"", ""Праздный"",
    ""Ребёнок"", ""Соперник"", ""Конформист"", ""Интриган"", ""Ворчун"",
    ""Девиант"", ""Режиссёр"", ""Загадка"", ""Глаз Бури"", ""Фанатик"",
    ""Галант"", ""Гуру"", ""Идеалист"", ""Судья"", ""Одиночка"",
    ""Мученик"", ""Мазохист"", ""Монстр"", ""Педагог"", ""Кающийся"",
    ""Перфекционист"", ""Бунтарь"", ""Плут"", ""Выживший"", ""Искатель приключений"",
    ""Традиционалист"", ""Трикстер"", ""Визионер""
  ],

  ""AttributeGroups"": [
    { ""Id"": ""mental"",   ""Name"": ""Ментальные"" },
    { ""Id"": ""physical"", ""Name"": ""Физические"" },
    { ""Id"": ""social"",   ""Name"": ""Социальные"" }
  ],
  ""Attributes"": [
    { ""Id"": ""intelligence"", ""GroupId"": ""mental"",   ""Name"": ""Интеллект"" },
    { ""Id"": ""wits"",         ""GroupId"": ""mental"",   ""Name"": ""Сообразительность"" },
    { ""Id"": ""resolve"",      ""GroupId"": ""mental"",   ""Name"": ""Решительность"" },
    { ""Id"": ""strength"",     ""GroupId"": ""physical"", ""Name"": ""Сила"" },
    { ""Id"": ""dexterity"",    ""GroupId"": ""physical"", ""Name"": ""Ловкость"" },
    { ""Id"": ""stamina"",      ""GroupId"": ""physical"", ""Name"": ""Выносливость"" },
    { ""Id"": ""charisma"",     ""GroupId"": ""social"",   ""Name"": ""Внушительность"" },
    { ""Id"": ""manipulation"", ""GroupId"": ""social"",   ""Name"": ""Манипулирование"" },
    { ""Id"": ""composure"",    ""GroupId"": ""social"",   ""Name"": ""Самообладание"" }
  ],

  ""SkillGroups"": [
    { ""Id"": ""mental"",   ""Name"": ""Ментальные"" },
    { ""Id"": ""physical"", ""Name"": ""Физические"" },
    { ""Id"": ""social"",   ""Name"": ""Социальные"" }
  ],
  ""Skills"": [
    { ""Id"": ""academics"",     ""GroupId"": ""mental"",   ""Name"": ""Гуманитарные науки"", ""AltByTimeFrame"": {} },
    { ""Id"": ""computers"",     ""GroupId"": ""mental"",   ""Name"": ""Компьютеры"",         ""AltByTimeFrame"": { ""DA"": ""Энигма"" } },
    { ""Id"": ""craft"",         ""GroupId"": ""mental"",   ""Name"": ""Ремесло"",            ""AltByTimeFrame"": {} },
    { ""Id"": ""investigation"", ""GroupId"": ""mental"",   ""Name"": ""Расследование"",      ""AltByTimeFrame"": {} },
    { ""Id"": ""occult"",        ""GroupId"": ""mental"",   ""Name"": ""Оккультизм"",         ""AltByTimeFrame"": {} },
    { ""Id"": ""politics"",      ""GroupId"": ""mental"",   ""Name"": ""Политика"",           ""AltByTimeFrame"": {} },
    { ""Id"": ""medicine"",      ""GroupId"": ""mental"",   ""Name"": ""Медицина"",           ""AltByTimeFrame"": {} },
    { ""Id"": ""science"",       ""GroupId"": ""mental"",   ""Name"": ""Естественные науки"", ""AltByTimeFrame"": {} },
    { ""Id"": ""athletics"",     ""GroupId"": ""physical"", ""Name"": ""Атлетика"",           ""AltByTimeFrame"": {} },
    { ""Id"": ""firearms"",      ""GroupId"": ""physical"", ""Name"": ""Стрельба"",           ""AltByTimeFrame"": {} },
    { ""Id"": ""brawl"",         ""GroupId"": ""physical"", ""Name"": ""Драка"",              ""AltByTimeFrame"": {} },
    { ""Id"": ""stealth"",       ""GroupId"": ""physical"", ""Name"": ""Скрытность"",         ""AltByTimeFrame"": {} },
    { ""Id"": ""drive"",         ""GroupId"": ""physical"", ""Name"": ""Вождение"",           ""AltByTimeFrame"": { ""DA"": ""Верховая езда"" } },
    { ""Id"": ""larceny"",       ""GroupId"": ""physical"", ""Name"": ""Воровство"",          ""AltByTimeFrame"": {} },
    { ""Id"": ""melee"",         ""GroupId"": ""physical"", ""Name"": ""Холодное оружие"",    ""AltByTimeFrame"": {} },
    { ""Id"": ""survival"",      ""GroupId"": ""physical"", ""Name"": ""Выживание"",          ""AltByTimeFrame"": {} },
    { ""Id"": ""animal_ken"",    ""GroupId"": ""social"",   ""Name"": ""Понимание животных"", ""AltByTimeFrame"": {} },
    { ""Id"": ""empathy"",       ""GroupId"": ""social"",   ""Name"": ""Эмпатия"",            ""AltByTimeFrame"": {} },
    { ""Id"": ""persuasion"",    ""GroupId"": ""social"",   ""Name"": ""Убеждение"",          ""AltByTimeFrame"": {} },
    { ""Id"": ""expression"",    ""GroupId"": ""social"",   ""Name"": ""Экспрессия"",         ""AltByTimeFrame"": {} },
    { ""Id"": ""streetwise"",    ""GroupId"": ""social"",   ""Name"": ""Уличное чутьё"",      ""AltByTimeFrame"": {} },
    { ""Id"": ""intimidation"",  ""GroupId"": ""social"",   ""Name"": ""Запугивание"",        ""AltByTimeFrame"": {} },
    { ""Id"": ""subterfuge"",    ""GroupId"": ""social"",   ""Name"": ""Хитрость"",           ""AltByTimeFrame"": {} },
    { ""Id"": ""etiquette"",     ""GroupId"": ""social"",   ""Name"": ""Общение"",            ""AltByTimeFrame"": {} }
  ],

  ""Ranks"": [
    {
      ""Id"": ""fledgling"", ""Name"": ""Птенец"", ""MaxPlayers"": 10,
      ""AttributePoints"": { ""Primary"": 4, ""Secondary"": 3, ""Tertiary"": 2 }, ""AttributeMax"": 4,
      ""SkillPoints"": { ""Primary"": 8, ""Secondary"": 5, ""Tertiary"": 3 }, ""SkillMax"": 2,
      ""SpecializationPoints"": 2, ""DisciplinePoints"": 2, ""MeritPoints"": 7, ""AddonPoints"": 3,
      ""StartBloodPotency"": 1, ""StartHumanity"": 7,
      ""FirstCharBonus"": { ""Xp"": 0, ""MeritIds"": [], ""ExtraAttributePoints"": 0, ""ExtraSkillPoints"": 0, ""ExtraDisciplinePoints"": 0 }
    },
    {
      ""Id"": ""neonate"", ""Name"": ""Неонат"", ""MaxPlayers"": 10,
      ""AttributePoints"": { ""Primary"": 5, ""Secondary"": 4, ""Tertiary"": 3 }, ""AttributeMax"": 5,
      ""SkillPoints"": { ""Primary"": 11, ""Secondary"": 7, ""Tertiary"": 4 }, ""SkillMax"": 3,
      ""SpecializationPoints"": 3, ""DisciplinePoints"": 3, ""MeritPoints"": 10, ""AddonPoints"": 5,
      ""StartBloodPotency"": 1, ""StartHumanity"": 7,
      ""FirstCharBonus"": { ""Xp"": 0, ""MeritIds"": [], ""ExtraAttributePoints"": 0, ""ExtraSkillPoints"": 0, ""ExtraDisciplinePoints"": 0 }
    },
    {
      ""Id"": ""ancilla"", ""Name"": ""Анцилла"", ""MaxPlayers"": 6,
      ""AttributePoints"": { ""Primary"": 7, ""Secondary"": 5, ""Tertiary"": 3 }, ""AttributeMax"": 5,
      ""SkillPoints"": { ""Primary"": 13, ""Secondary"": 9, ""Tertiary"": 5 }, ""SkillMax"": 4,
      ""SpecializationPoints"": 4, ""DisciplinePoints"": 4, ""MeritPoints"": 15, ""AddonPoints"": 8,
      ""StartBloodPotency"": 2, ""StartHumanity"": 7,
      ""FirstCharBonus"": { ""Xp"": 0, ""MeritIds"": [], ""ExtraAttributePoints"": 0, ""ExtraSkillPoints"": 0, ""ExtraDisciplinePoints"": 0 }
    },
    {
      ""Id"": ""elder"", ""Name"": ""Старейшина"", ""MaxPlayers"": 3,
      ""AttributePoints"": { ""Primary"": 9, ""Secondary"": 7, ""Tertiary"": 5 }, ""AttributeMax"": 6,
      ""SkillPoints"": { ""Primary"": 15, ""Secondary"": 11, ""Tertiary"": 7 }, ""SkillMax"": 5,
      ""SpecializationPoints"": 5, ""DisciplinePoints"": 5, ""MeritPoints"": 20, ""AddonPoints"": 10,
      ""StartBloodPotency"": 4, ""StartHumanity"": 6,
      ""FirstCharBonus"": { ""Xp"": 0, ""MeritIds"": [], ""ExtraAttributePoints"": 0, ""ExtraSkillPoints"": 0, ""ExtraDisciplinePoints"": 0 }
    },
    {
      ""Id"": ""methuselah"", ""Name"": ""Мафусаил"", ""MaxPlayers"": 1,
      ""AttributePoints"": { ""Primary"": 10, ""Secondary"": 8, ""Tertiary"": 6 }, ""AttributeMax"": 8,
      ""SkillPoints"": { ""Primary"": 18, ""Secondary"": 13, ""Tertiary"": 8 }, ""SkillMax"": 6,
      ""SpecializationPoints"": 6, ""DisciplinePoints"": 6, ""MeritPoints"": 25, ""AddonPoints"": 12,
      ""StartBloodPotency"": 6, ""StartHumanity"": 5,
      ""FirstCharBonus"": { ""Xp"": 0, ""MeritIds"": [], ""ExtraAttributePoints"": 0, ""ExtraSkillPoints"": 0, ""ExtraDisciplinePoints"": 0 }
    },
    {
      ""Id"": ""blood_god"", ""Name"": ""Кровавый Бог"", ""MaxPlayers"": 1,
      ""AttributePoints"": { ""Primary"": 10, ""Secondary"": 10, ""Tertiary"": 8 }, ""AttributeMax"": 10,
      ""SkillPoints"": { ""Primary"": 20, ""Secondary"": 15, ""Tertiary"": 10 }, ""SkillMax"": 8,
      ""SpecializationPoints"": 8, ""DisciplinePoints"": 8, ""MeritPoints"": 30, ""AddonPoints"": 15,
      ""StartBloodPotency"": 9, ""StartHumanity"": 3,
      ""FirstCharBonus"": { ""Xp"": 0, ""MeritIds"": [], ""ExtraAttributePoints"": 0, ""ExtraSkillPoints"": 0, ""ExtraDisciplinePoints"": 0 }
    }
  ],

  ""BloodPotencyBloodMax"": {
    ""1"": 10, ""2"": 11, ""3"": 12, ""4"": 13, ""5"": 15,
    ""6"": 20, ""7"": 30, ""8"": 40, ""9"": 50, ""10"": 60
  },

  ""BloodPotencyMaxRating"": {
    ""1"": 5, ""2"": 5, ""3"": 5, ""4"": 6, ""5"": 6,
    ""6"": 7, ""7"": 7, ""8"": 8, ""9"": 9, ""10"": 10
  },

  ""BloodPotencyHealRate"": {
    ""1"": 1, ""2"": 1, ""3"": 1, ""4"": 1, ""5"": 1,
    ""6"": 1, ""7"": 1, ""8"": 1, ""9"": 1, ""10"": 1
  },

  ""Disciplines"": [
    {
      ""Id"": ""auspex"", ""Name"": ""Ауспекс"", ""Description"": ""Сверхъестественное восприятие."",
      ""IsPath"": false, ""ParentDisciplineId"": """", ""IsAmalgam"": false, ""AmalgamRequirements"": [],
      ""Abilities"": [
        { ""Level"": 1, ""Name"": ""Обострённые чувства"", ""Description"": ""Усиление одного чувства."", ""Type"": ""Active"", ""RollParams"": [""attr.wits"", ""skill.empathy""], ""AutoLearn"": true },
        { ""Level"": 2, ""Name"": ""Чтение ауры"", ""Description"": ""Видение ауры цели."", ""Type"": ""Active"", ""RollParams"": [""attr.wits"", ""skill.empathy""], ""AutoLearn"": true },
        { ""Level"": 3, ""Name"": ""Взгляд в прошлое"", ""Description"": ""Прикосновение к воспоминаниям."", ""Type"": ""Active"", ""RollParams"": [""attr.intelligence"", ""skill.investigation""], ""AutoLearn"": true },
        { ""Level"": 4, ""Name"": ""Телепатия"", ""Description"": ""Чтение мыслей."", ""Type"": ""Active"", ""RollParams"": [""attr.intelligence"", ""skill.occult""], ""AutoLearn"": true },
        { ""Level"": 5, ""Name"": ""Астральная проекция"", ""Description"": ""Выход духа из тела."", ""Type"": ""Active"", ""RollParams"": [""attr.intelligence"", ""skill.occult""], ""AutoLearn"": true }
      ]
    },
    {
      ""Id"": ""celerity"", ""Name"": ""Стремительность"", ""Description"": ""Сверхчеловеческая скорость."",
      ""IsPath"": false, ""ParentDisciplineId"": """", ""IsAmalgam"": false, ""AmalgamRequirements"": [],
      ""Abilities"": [
        { ""Level"": 1, ""Name"": ""Кошачья грация"", ""Description"": ""Бонус к Ловкости."", ""Type"": ""Passive"", ""RollParams"": [], ""AutoLearn"": true },
        { ""Level"": 2, ""Name"": ""Быстрая реакция"", ""Description"": ""Ускоренная реакция."", ""Type"": ""Passive"", ""RollParams"": [], ""AutoLearn"": true },
        { ""Level"": 3, ""Name"": ""Всплеск скорости"", ""Description"": ""Дополнительное действие."", ""Type"": ""Active"", ""RollParams"": [], ""AutoLearn"": true },
        { ""Level"": 4, ""Name"": ""Молниеносный удар"", ""Description"": ""Атака до реакции противника."", ""Type"": ""Active"", ""RollParams"": [], ""AutoLearn"": true },
        { ""Level"": 5, ""Name"": ""Временной скачок"", ""Description"": ""Ускорение времени."", ""Type"": ""Active"", ""RollParams"": [], ""AutoLearn"": true }
      ]
    },
    {
      ""Id"": ""dominate"", ""Name"": ""Доминирование"", ""Description"": ""Власть над разумом."",
      ""IsPath"": false, ""ParentDisciplineId"": """", ""IsAmalgam"": false, ""AmalgamRequirements"": [],
      ""Abilities"": [
        { ""Level"": 1, ""Name"": ""Внушение"", ""Description"": ""Короткий приказ."", ""Type"": ""Active"", ""RollParams"": [""attr.charisma"", ""skill.intimidation"", ""disc.dominate""], ""AutoLearn"": true },
        { ""Level"": 2, ""Name"": ""Мезмеризм"", ""Description"": ""Погружение в транс."", ""Type"": ""Active"", ""RollParams"": [""attr.charisma"", ""skill.subterfuge"", ""disc.dominate""], ""AutoLearn"": true },
        { ""Level"": 3, ""Name"": ""Забыть"", ""Description"": ""Стирание памяти."", ""Type"": ""Active"", ""RollParams"": [""attr.manipulation"", ""skill.subterfuge"", ""disc.dominate""], ""AutoLearn"": true },
        { ""Level"": 4, ""Name"": ""Разум-крепость"", ""Description"": ""Полный контроль."", ""Type"": ""Active"", ""RollParams"": [""attr.manipulation"", ""skill.intimidation"", ""disc.dominate""], ""AutoLearn"": true },
        { ""Level"": 5, ""Name"": ""Смена личности"", ""Description"": ""Изменение личности."", ""Type"": ""Active"", ""RollParams"": [""attr.manipulation"", ""skill.subterfuge"", ""disc.dominate""], ""AutoLearn"": true }
      ]
    },
    {
      ""Id"": ""fortitude"", ""Name"": ""Стойкость"", ""Description"": ""Сверхъестественная выживаемость."",
      ""IsPath"": false, ""ParentDisciplineId"": """", ""IsAmalgam"": false, ""AmalgamRequirements"": [],
      ""Abilities"": [
        { ""Level"": 1, ""Name"": ""Живучесть"", ""Description"": ""Снижение урона."", ""Type"": ""Passive"", ""RollParams"": [], ""AutoLearn"": true },
        { ""Level"": 2, ""Name"": ""Каменная кожа"", ""Description"": ""Дополнительное поглощение."", ""Type"": ""Passive"", ""RollParams"": [], ""AutoLearn"": true },
        { ""Level"": 3, ""Name"": ""Упорство"", ""Description"": ""Сопротивление эффектам."", ""Type"": ""Passive"", ""RollParams"": [], ""AutoLearn"": true },
        { ""Level"": 4, ""Name"": ""Неуязвимость"", ""Description"": ""Снижение критического урона."", ""Type"": ""Passive"", ""RollParams"": [], ""AutoLearn"": true },
        { ""Level"": 5, ""Name"": ""Непоколебимость"", ""Description"": ""Иммунитет к типу урона."", ""Type"": ""Passive"", ""RollParams"": [], ""AutoLearn"": true }
      ]
    },
    {
      ""Id"": ""obfuscate"", ""Name"": ""Сокрытие"", ""Description"": ""Управление восприятием."",
      ""IsPath"": false, ""ParentDisciplineId"": """", ""IsAmalgam"": false, ""AmalgamRequirements"": [],
      ""Abilities"": [
        { ""Level"": 1, ""Name"": ""Тень"", ""Description"": ""Слияние с тенями."", ""Type"": ""Active"", ""RollParams"": [""attr.wits"", ""skill.stealth""], ""AutoLearn"": true },
        { ""Level"": 2, ""Name"": ""Невидимость"", ""Description"": ""Исчезновение из вида."", ""Type"": ""Active"", ""RollParams"": [""attr.wits"", ""skill.stealth""], ""AutoLearn"": true },
        { ""Level"": 3, ""Name"": ""Маска"", ""Description"": ""Изменение внешности."", ""Type"": ""Active"", ""RollParams"": [""attr.manipulation"", ""skill.subterfuge""], ""AutoLearn"": true },
        { ""Level"": 4, ""Name"": ""Исчезновение"", ""Description"": ""Полное исчезновение."", ""Type"": ""Active"", ""RollParams"": [""attr.wits"", ""skill.stealth""], ""AutoLearn"": true },
        { ""Level"": 5, ""Name"": ""Забвение"", ""Description"": ""Стирание из восприятия."", ""Type"": ""Active"", ""RollParams"": [""attr.wits"", ""skill.subterfuge""], ""AutoLearn"": true }
      ]
    },
    {
      ""Id"": ""potence"", ""Name"": ""Мощь"", ""Description"": ""Сверхъестественная сила."",
      ""IsPath"": false, ""ParentDisciplineId"": """", ""IsAmalgam"": false, ""AmalgamRequirements"": [],
      ""Abilities"": [
        { ""Level"": 1, ""Name"": ""Силач"", ""Description"": ""Бонус к Силе."", ""Type"": ""Passive"", ""RollParams"": [], ""AutoLearn"": true },
        { ""Level"": 2, ""Name"": ""Разрушительный удар"", ""Description"": ""Мощная атака."", ""Type"": ""Active"", ""RollParams"": [""attr.strength"", ""skill.brawl""], ""AutoLearn"": true },
        { ""Level"": 3, ""Name"": ""Неудержимость"", ""Description"": ""Пробитие защиты."", ""Type"": ""Passive"", ""RollParams"": [], ""AutoLearn"": true },
        { ""Level"": 4, ""Name"": ""Сокрушение"", ""Description"": ""Уничтожение преград."", ""Type"": ""Active"", ""RollParams"": [""attr.strength""], ""AutoLearn"": true },
        { ""Level"": 5, ""Name"": ""Титанический удар"", ""Description"": ""Разрушительная атака."", ""Type"": ""Active"", ""RollParams"": [""attr.strength"", ""skill.brawl""], ""AutoLearn"": true }
      ]
    },
    {
      ""Id"": ""presence"", ""Name"": ""Присутствие"", ""Description"": ""Влияние на эмоции."",
      ""IsPath"": false, ""ParentDisciplineId"": """", ""IsAmalgam"": false, ""AmalgamRequirements"": [],
      ""Abilities"": [
        { ""Level"": 1, ""Name"": ""Очарование"", ""Description"": ""Привлечение внимания."", ""Type"": ""Active"", ""RollParams"": [""attr.charisma"", ""skill.persuasion""], ""AutoLearn"": true },
        { ""Level"": 2, ""Name"": ""Величественность"", ""Description"": ""Вызов благоговения."", ""Type"": ""Passive"", ""RollParams"": [], ""AutoLearn"": true },
        { ""Level"": 3, ""Name"": ""Паника"", ""Description"": ""Внушение страха."", ""Type"": ""Active"", ""RollParams"": [""attr.charisma"", ""skill.intimidation""], ""AutoLearn"": true },
        { ""Level"": 4, ""Name"": ""Смятение"", ""Description"": ""Вызов ярости."", ""Type"": ""Active"", ""RollParams"": [""attr.manipulation"", ""skill.intimidation""], ""AutoLearn"": true },
        { ""Level"": 5, ""Name"": ""Подчинение"", ""Description"": ""Власть над эмоциями."", ""Type"": ""Active"", ""RollParams"": [""attr.manipulation"", ""skill.persuasion""], ""AutoLearn"": true }
      ]
    },
    {
      ""Id"": ""protean"", ""Name"": ""Протеанство"", ""Description"": ""Изменение плоти."",
      ""IsPath"": false, ""ParentDisciplineId"": """", ""IsAmalgam"": false, ""AmalgamRequirements"": [],
      ""Abilities"": [
        { ""Level"": 1, ""Name"": ""Кошачьи глаза"", ""Description"": ""Ночное зрение."", ""Type"": ""Passive"", ""RollParams"": [], ""AutoLearn"": true },
        { ""Level"": 2, ""Name"": ""Звериные когти"", ""Description"": ""Естественное оружие."", ""Type"": ""Active"", ""RollParams"": [""attr.strength"", ""skill.brawl""], ""AutoLearn"": true },
        { ""Level"": 3, ""Name"": ""Слияние с землёй"", ""Description"": ""Слияние с почвой."", ""Type"": ""Active"", ""RollParams"": [""attr.wits"", ""skill.survival""], ""AutoLearn"": true },
        { ""Level"": 4, ""Name"": ""Форма зверя"", ""Description"": ""Превращение в зверя."", ""Type"": ""Active"", ""RollParams"": [""attr.stamina"", ""skill.survival""], ""AutoLearn"": true },
        { ""Level"": 5, ""Name"": ""Туман"", ""Description"": ""Превращение в туман."", ""Type"": ""Active"", ""RollParams"": [""attr.stamina"", ""skill.survival""], ""AutoLearn"": true }
      ]
    },
    {
      ""Id"": ""animalism"", ""Name"": ""Анимализм"", ""Description"": ""Связь с животными."",
      ""IsPath"": false, ""ParentDisciplineId"": """", ""IsAmalgam"": false, ""AmalgamRequirements"": [],
      ""Abilities"": [
        { ""Level"": 1, ""Name"": ""Разговор с животными"", ""Description"": ""Общение с животными."", ""Type"": ""Active"", ""RollParams"": [""attr.charisma"", ""skill.animal_ken""], ""AutoLearn"": true },
        { ""Level"": 2, ""Name"": ""Призыв"", ""Description"": ""Призыв животного."", ""Type"": ""Active"", ""RollParams"": [""attr.charisma"", ""skill.animal_ken""], ""AutoLearn"": true },
        { ""Level"": 3, ""Name"": ""Внушение зверю"", ""Description"": ""Приказы животному."", ""Type"": ""Active"", ""RollParams"": [""attr.manipulation"", ""skill.animal_ken""], ""AutoLearn"": true },
        { ""Level"": 4, ""Name"": ""Слияние"", ""Description"": ""Вселение в животное."", ""Type"": ""Active"", ""RollParams"": [""attr.intelligence"", ""skill.animal_ken""], ""AutoLearn"": true },
        { ""Level"": 5, ""Name"": ""Всеобщий зов"", ""Description"": ""Призыв всех животных."", ""Type"": ""Active"", ""RollParams"": [""attr.charisma"", ""skill.animal_ken""], ""AutoLearn"": true }
      ]
    },
    {
      ""Id"": ""thaumaturgy"", ""Name"": ""Тауматургия"", ""Description"": ""Кровавая магия Тремер."",
      ""IsPath"": false, ""ParentDisciplineId"": """", ""IsAmalgam"": false, ""AmalgamRequirements"": [],
      ""Abilities"": [
        { ""Level"": 1, ""Name"": ""Вкус крови"", ""Description"": ""Определение свойств крови."", ""Type"": ""Active"", ""RollParams"": [""attr.intelligence"", ""skill.occult"", ""disc.thaumaturgy""], ""AutoLearn"": true },
        { ""Level"": 2, ""Name"": ""Кровавый щит"", ""Description"": ""Магическая защита."", ""Type"": ""Active"", ""RollParams"": [""attr.intelligence"", ""skill.occult"", ""disc.thaumaturgy""], ""AutoLearn"": true },
        { ""Level"": 3, ""Name"": ""Ритуал крови"", ""Description"": ""Проведение ритуала."", ""Type"": ""Active"", ""RollParams"": [""attr.intelligence"", ""skill.occult"", ""disc.thaumaturgy""], ""AutoLearn"": true },
        { ""Level"": 4, ""Name"": ""Кровавая связь"", ""Description"": ""Магическая связь."", ""Type"": ""Active"", ""RollParams"": [""attr.intelligence"", ""skill.occult"", ""disc.thaumaturgy""], ""AutoLearn"": true },
        { ""Level"": 5, ""Name"": ""Овладение кровью"", ""Description"": ""Управление кровью."", ""Type"": ""Active"", ""RollParams"": [""attr.intelligence"", ""skill.occult"", ""disc.thaumaturgy""], ""AutoLearn"": true }
      ]
    }
  ],

  ""Clans"": [
    {
      ""Id"": ""ventrue"", ""Name"": ""Вентру"", ""Description"": ""Клан королей и тиранов."",
      ""Curse"": ""Ограниченный вкус: питается только определённым типом смертных."",
      ""ClanDisciplineIds"": [""dominate"", ""fortitude"", ""presence""],
      ""FavoredAttributeIds"": [""composure"", ""resolve""],
      ""FavoredSkillIds"": [""persuasion"", ""etiquette"", ""intimidation"", ""politics""]
    },
    {
      ""Id"": ""tremere"", ""Name"": ""Тремер"", ""Description"": ""Клан магов и чародеев."",
      ""Curse"": ""Аура отверженности: Вентру и Ассамиты ненавидят Тремер."",
      ""ClanDisciplineIds"": [""auspex"", ""dominate"", ""thaumaturgy""],
      ""FavoredAttributeIds"": [""intelligence"", ""resolve""],
      ""FavoredSkillIds"": [""occult"", ""academics"", ""science"", ""investigation""]
    },
    {
      ""Id"": ""brujah"", ""Name"": ""Бруха"", ""Description"": ""Клан воинов и бунтарей."",
      ""Curse"": ""Гнев: сложнее сопротивляться Безумию."",
      ""ClanDisciplineIds"": [""celerity"", ""potence"", ""presence""],
      ""FavoredAttributeIds"": [""strength"", ""charisma""],
      ""FavoredSkillIds"": [""athletics"", ""brawl"", ""intimidation"", ""persuasion""]
    },
    {
      ""Id"": ""gangrel"", ""Name"": ""Гангрел"", ""Description"": ""Клан зверей и выживших."",
      ""Curse"": ""Звериные черты: при Безумии приобретает звериные черты."",
      ""ClanDisciplineIds"": [""animalism"", ""fortitude"", ""protean""],
      ""FavoredAttributeIds"": [""stamina"", ""wits""],
      ""FavoredSkillIds"": [""survival"", ""animal_ken"", ""athletics"", ""stealth""]
    },
    {
      ""Id"": ""nosferatu"", ""Name"": ""Носферату"", ""Description"": ""Клан уродов и шпионов."",
      ""Curse"": ""Уродство: 0 успехов при социальных бросках с внешностью."",
      ""ClanDisciplineIds"": [""animalism"", ""obfuscate"", ""potence""],
      ""FavoredAttributeIds"": [""wits"", ""strength""],
      ""FavoredSkillIds"": [""stealth"", ""streetwise"", ""animal_ken"", ""larceny""]
    },
    {
      ""Id"": ""toreador"", ""Name"": ""Тореадор"", ""Description"": ""Клан художников и гедонистов."",
      ""Curse"": ""Восприятие красоты: застывают при виде прекрасного."",
      ""ClanDisciplineIds"": [""auspex"", ""celerity"", ""presence""],
      ""FavoredAttributeIds"": [""charisma"", ""composure""],
      ""FavoredSkillIds"": [""expression"", ""empathy"", ""persuasion"", ""etiquette""]
    },
    {
      ""Id"": ""malkavian"", ""Name"": ""Малкавиан"", ""Description"": ""Клан безумцев и пророков."",
      ""Curse"": ""Безумие: у каждого Малкавиана постоянное психическое расстройство."",
      ""ClanDisciplineIds"": [""auspex"", ""dominate"", ""obfuscate""],
      ""FavoredAttributeIds"": [""wits"", ""intelligence""],
      ""FavoredSkillIds"": [""occult"", ""investigation"", ""subterfuge"", ""empathy""]
    }
  ],

  ""Bloodlines"": [],
  ""Merits"": [],
  ""Moralites"": [
    {
      ""Id"": ""humanity"", ""Name"": ""Человечность"", ""StartRating"": 7,
      ""Sins"": [
        { ""Level"": 10, ""Text"": ""Самоконтроль (мелкая провинность)"" },
        { ""Level"": 9,  ""Text"": ""Мелкие эгоистичные поступки"" },
        { ""Level"": 8,  ""Text"": ""Оскорбление или унижение"" },
        { ""Level"": 7,  ""Text"": ""Мелкая кража или вандализм"" },
        { ""Level"": 6,  ""Text"": ""Нанесение тяжких телесных повреждений"" },
        { ""Level"": 5,  ""Text"": ""Убийство в порядке самозащиты"" },
        { ""Level"": 4,  ""Text"": ""Намеренное убийство"" },
        { ""Level"": 3,  ""Text"": ""Массовое убийство"" },
        { ""Level"": 2,  ""Text"": ""Убийство близкого человека"" },
        { ""Level"": 1,  ""Text"": ""Хладнокровное убийство ради удовольствия"" }
      ],
      ""AuraName"": ""Человечность"",
      ""AuraAffectedParams"": [""skill.empathy"", ""skill.persuasion""]
    }
  ],

  ""Addons"": [],

  ""XpSystem"": {
    ""Mode"": ""CurrentValues"",
    ""Costs"": {
      ""Attribute"": 4, ""FavoredAttribute"": 3,
      ""Skill"": 2, ""FavoredSkill"": 1,
      ""Discipline"": 5, ""ClanDiscipline"": 4,
      ""Addon"": 3, ""Willpower"": 1
    }
  },

  ""RollPresets"": [
    {
      ""Name"": ""Атака холодным оружием"",
      ""Params"": [""attr.strength"", ""skill.melee""],
      ""Modifier"": 0, ""Difficulty"": 0,
      ""Type"": ""Regular"", ""Extended"": false, ""TargetSuccesses"": 0
    },
    {
      ""Name"": ""Сопротивление Доминированию"",
      ""Params"": [""attr.resolve"", ""attr.composure""],
      ""Modifier"": 0, ""Difficulty"": 0,
      ""Type"": ""Resistance"", ""Extended"": false, ""TargetSuccesses"": 0
    }
  ]
}
";

        public const string AuraTableJson = @"
{
  ""Rows"": [
    { ""MoralityRating"": 10, ""Weak"":  2, ""Regular"":  3, ""Strong"":  5 },
    { ""MoralityRating"":  9, ""Weak"":  1, ""Regular"":  2, ""Strong"":  3 },
    { ""MoralityRating"":  8, ""Weak"":  0, ""Regular"":  1, ""Strong"":  2 },
    { ""MoralityRating"":  7, ""Weak"":  0, ""Regular"":  0, ""Strong"":  1 },
    { ""MoralityRating"":  6, ""Weak"":  0, ""Regular"":  0, ""Strong"":  0 },
    { ""MoralityRating"":  5, ""Weak"":  0, ""Regular"":  0, ""Strong"":  0 },
    { ""MoralityRating"":  4, ""Weak"":  0, ""Regular"":  0, ""Strong"": -1 },
    { ""MoralityRating"":  3, ""Weak"":  0, ""Regular"": -1, ""Strong"": -2 },
    { ""MoralityRating"":  2, ""Weak"": -1, ""Regular"": -2, ""Strong"": -3 },
    { ""MoralityRating"":  1, ""Weak"": -2, ""Regular"": -3, ""Strong"": -5 },
    { ""MoralityRating"":  0, ""Weak"":  0, ""Regular"":  0, ""Strong"":  0 }
  ]
}
";
    }
}