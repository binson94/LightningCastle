namespace ENUM
{
    public enum Shape
    {
        Spade = 1, Diamond = 2, Clover = 3, Heart = 4
    }

    public enum Color
    {
        None = 0, Red = 5, Orange = 10, Yellow = 15, Green = 20, Blue = 25, Navy = 30, Purple = 35
    }
}

public enum EnemyStat
{
    AttackType, Level, HPMax, AttackMin, AttackMax, Range, AttackSpd, PhyDefence, MagDefence,
    MoveSpeed, KnockbackReg, DropRate, DropBonus, nDust, nJem, cDust, cJem
}

public enum Type
{
    Physical, Magical
}

public enum CharClass
{
    Dummy, Hero, Chaser, Crusher, Guardian, Alchemist, Astronomer, Spirit
}

//Special에 포함 : Crusher의 OverHeatAttack
public enum Anim
{
    Idle, Move, Attack, Skill1, Skill2, Jump, Special
}

public enum CharStat
{
    HPMax, AttackMin, AttackMax, ClawAttackMin, ClawAttackMax,
    PhysicalDefense, MagicDefense, AttackSpeed, ClawAttackSpeed, MovementSpeed
}

public enum Area
{
    Outer, BalconyUnder, Floor1Right, Floor1Left, Balcony, Floor2
}