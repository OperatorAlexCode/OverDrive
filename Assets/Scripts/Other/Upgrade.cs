using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Upgrade : ScriptableObject
{
    //[SerializeField]
    public UpgradeType Type;
    //[SerializeField]
    public List<StatChange> Changes = new();
    public Sprite Icon;
}

[Serializable]
public class StatChange
{
    [SerializeField]
    public float Value; /*{ get; set; }*/
    public Stat Stat;
}

public enum UpgradeType
{
    Ship,
    Laser,
    Torpedo,
    ScatterShot,
    ChargedShot,
    Missile,
    Flak
}

public enum Stat
{
    Health,
    Acceleration,
    MaxVelocity,
    MaxHeat,
    Damage,
    FireVelocity,
    HeatCost,
    Spread,
    ShotsAmount,
    Penetration,
    DamageRange,
    WeaponUnlock
}

//public enum ShipStat
//{
//    Health,
//    Speed,
//    MaxVelocity,
//    MaxHeat,
//    //HeatGain,
//    //HeatLoss,
//    RotateSpeed,
//    SlowDownForce
//}

//public enum WeaponStat
//{
//    // Universal
//    Damage,
//    FireVelocity,
//    HeatCost,
//    FireDelay,
//    // Scattershot
//    Spread,
//    ShotsAmount,
//    // Chargedshot
//    Penetration,
//    // Missile
//    Speed,
//    MaxVelocity,
//    RotateSpeed,
//    SlowDownForce,
//    // Flak
//    DamageRange,
//    ExplodeDistance
//}