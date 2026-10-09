namespace VTR.Core.Models
{
    /// <summary>
    /// Тип урона, наносимый персонажу.
    /// </summary>
    public enum DamageType
    {
        None = 0,       // здоровая клетка
        Bashing = 1,    // ударный урон (дробящий) — / 
        Lethal = 2,     // летальный урон (пули, ножи) — X
        Aggravated = 3  // агрегированный урон (огонь, солнце, клыки) — *
    }
}