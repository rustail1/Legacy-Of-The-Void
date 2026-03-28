using System;
using UnityEngine;

[Serializable]
public class BattleFighterConfig
{
    public MasterRank rank = MasterRank.ThirdRate;

    [Header("Manual Stats")]
    public bool useManualStats = false;
    public int bodyLevel = 0;
    public int qiLevel = 0;

    [Header("Optional Start Overrides")]
    public bool overrideStartHp = false;
    public int startHp = 50;

    public bool overrideStartQi = false;
    public int startQi = 20;

    [Header("Available Techniques")]
    public bool dragonFist = true;
    public bool craneKick = true;
    public bool moonSlash = true;
    public bool voidPalm = true;
}

[Serializable]
public class BattleLaunchData
{
    public BattleFighterConfig player = new BattleFighterConfig();
    public BattleFighterConfig enemy = new BattleFighterConfig();
}
