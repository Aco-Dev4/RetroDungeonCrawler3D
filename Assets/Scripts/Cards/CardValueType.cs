public enum CardValueDisplayType
{
    FlatNumber,              // 20 -> 30
    PercentFromDecimal,      // 0.03 -> 3%
    Multiplier,              // 0.5 -> 1.5x
    FinalFlatStat,           // player 1 jump -> 2 jumps
    FinalPercentStat,        // player 1% crit -> 3%
    FinalMultiplierStat      // player 1.5x crit dmg -> 2x
}