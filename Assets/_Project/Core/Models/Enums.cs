namespace VTR.Core.Models
{
    public enum XpMode
    {
        Flat,           // фиксированная стоимость
        CurrentValues,  // текущее значение * стоимость
        NextValues      // (текущее+1) * стоимость
    }

    public enum RollOutcome
    {
        Fail,
        Success,
        CritSuccess,
        CritFail,
        FateRoll
    }

    public enum RollType
    {
        Regular,      // обычный
        Competitive,  // соревновательный
        Resistance    // сопротивление
    }

    public enum AbilityType
    {
        Active,
        Passive
    }

    public enum AuraType
    {
        Weak,
        Regular,
        Strong
    }

    public enum BloodlineBonusType
    {
        ClanDiscipline,
        RegularDiscipline,
        Amalgam,
        Merit
    }
}