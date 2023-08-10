using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class UpgradeManager : MonoBehaviour
{
    public List<Upgrade> AvailableUpgrades = new();
    public List<Upgrade> ScatterShotUpgrades = new();
    public List<Upgrade> ChargedShotUpgrades = new();
    public List<Upgrade> MissileUpgrades = new();
    public List<Upgrade> FlakUpgrades = new();
    public List<Upgrade> AquiredUpgrades = new();
    [SerializeField]
    public List<WaveUpgradePair> SpecialWaveUpgrades;

    public List<Upgrade> GetUpgradeSet(int upgradeToGet)
    {
        if (SpecialWaveUpgrades.Any(vp => vp.Wave == GetComponent<GameManager>().Wave))
            return SpecialWaveUpgrades.Find(vp => vp.Wave == GetComponent<GameManager>().Wave).Upgrades;

        List<Upgrade> upgrades = new List<Upgrade>();
        List<Upgrade> output = new List<Upgrade>();
        upgrades.AddRange(AvailableUpgrades);

        for (int x = 0; x < upgradeToGet; x++)
        {
            if (upgrades.Count == 0)
                break;

            int selectedUpgrade = Random.Range(0, upgrades.Count);

            while (upgrades[selectedUpgrade].Changes.Any(c => c.Stat == Stat.WeaponUnlock && AquiredUpgrades.Any(u => u.Changes.Contains(c))))
                selectedUpgrade = Random.Range(0, upgrades.Count);

            output.Add(upgrades[selectedUpgrade]);
            upgrades.RemoveAt(selectedUpgrade);
        }

        return output;
    }

    public void UnlockWeaponUpgrades(PrimaryWeapon unlock)
    {
        switch (unlock)
        {
            case PrimaryWeapon.ScatterShot:
                AvailableUpgrades.AddRange(ScatterShotUpgrades);
                break;
            case PrimaryWeapon.ChargedShot:
                AvailableUpgrades.AddRange(ChargedShotUpgrades);
                break;
        }
    }

    public void UnlockWeaponUpgrades(SecondaryWeapon unlock)
    {
        switch (unlock)
        {
            case SecondaryWeapon.Missile:
                AvailableUpgrades.AddRange(MissileUpgrades);
                break;
            case SecondaryWeapon.Flak:
                AvailableUpgrades.AddRange(FlakUpgrades);
                break;
        }
    }

    [Serializable]
    public class WaveUpgradePair
    {
        [SerializeField]
        public int Wave;
        [SerializeField]
        public List<Upgrade> Upgrades = new();
    }
}
