using UnityEngine;

public enum Herostate
{
    Combat,
    Fallen,
    Reviving
}

public enum BoardState
{
    Idle,
    Executing,
    Unstable,
    SkillTargeting
}

public enum SkillType
{
    Passive,
    Active,
    Ultimate,
    Support
}

public enum DamageType
{
    Physical,
    Magical,
    TrueDamage
}

public enum RuneType
{
    Red, Blue, Green, Yellow, Orange, Purple, None, SpecialLineBlast, SpecialBomb, SpecialRainbow, SpecialMeteor
}

public enum SpecialRuneType
{
    None, Bomb, LineBlast, Rainbow, Meteor
}

public enum RuneEffect
{
    None, Frozen, Burn, Nullified, Empowered, PoisonSpread, Poison
}

public enum MatchType
{
    None, Match3Line, Match4Line, Match4Square, Match5Line, Match5Cross, Match6Plus
}
public enum StatusType
{
    None, Burn, Poison, Vulnerable, Stun, Silence, ReducedHealing, Unstoppable, Immortal
}