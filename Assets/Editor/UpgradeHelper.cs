using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;

public class UpgradeHelper : EditorWindow
{
    Upgrade NewUpgrade;
    Stat Stat;
    UpgradeType UpgradeType;
    int SelectedUpgradeList;
    float Value;
    Sprite UpgradeIcon;

    [MenuItem("Custom Tools/Upgrade Helper")]
    public static void ShowWindow()
    {
        GetWindow(typeof(UpgradeHelper));
    }

    private void OnEnable()
    {
        NewUpgrade = CreateInstance<Upgrade>();
    }

    private void OnGUI()
    {
        if (NewUpgrade == null)
            NewUpgrade = CreateInstance<Upgrade>();

        UpgradeManager mngr = GameObject.Find("Managers").GetComponent<UpgradeManager>();

        EditorGUILayout.PropertyField(new SerializedObject(NewUpgrade).FindProperty("Changes"), true);

        List<string> upgradeListNames = new();

        foreach (FieldInfo list in mngr.GetType().GetFields())
            upgradeListNames.Add(list.Name);

        SelectedUpgradeList = EditorGUILayout.Popup("Upgrade List", SelectedUpgradeList, upgradeListNames.ToArray());
        UpgradeType = (UpgradeType)EditorGUILayout.EnumPopup("Type", UpgradeType);

        GUILayout.Space(10f);
        Stat = (Stat)EditorGUILayout.EnumPopup("Stat", Stat);
        Value = EditorGUILayout.FloatField("Value", Value);

        if (GUILayout.Button("Add Change"))
        {
            StatChange newChange = new()
            {
                Stat = Stat,
                Value = Value
            };

            NewUpgrade.Changes.Add(newChange);
        }

        GUILayout.Space(10f);

        if (GUILayout.Button("Clear Changes"))
            NewUpgrade = CreateInstance<Upgrade>();

        if (GUILayout.Button("Add Upgrade"))
            AddUpgrade();

        UpgradeIcon = EditorGUILayout.ObjectField("Icon", UpgradeIcon, typeof(Sprite), false, GUILayout.Height(EditorGUIUtility.singleLineHeight)) as Sprite;
    }

    void AddUpgrade()
    {
        NewUpgrade.Type = UpgradeType;
        NewUpgrade.Icon = UpgradeIcon;

        switch (SelectedUpgradeList)
        {
            case 0:
                GameObject.Find("Managers").GetComponent<UpgradeManager>().AvailableUpgrades.Add(NewUpgrade);
                break;
            case 1:
                GameObject.Find("Managers").GetComponent<UpgradeManager>().ScatterShotUpgrades.Add(NewUpgrade);
                break;
            case 2:
                GameObject.Find("Managers").GetComponent<UpgradeManager>().ChargedShotUpgrades.Add(NewUpgrade);
                break;
            case 3:
                GameObject.Find("Managers").GetComponent<UpgradeManager>().MissileUpgrades.Add(NewUpgrade);
                break;
            case 4:
                GameObject.Find("Managers").GetComponent<UpgradeManager>().FlakUpgrades.Add(NewUpgrade);
                break;
            case 6:
                List<UpgradeManager.WaveUpgradePair> specialUpgradeList = GameObject.Find("Managers").GetComponent<UpgradeManager>().SpecialWaveUpgrades;

                // If a WaveUpgradePair already exist for a specific wave, then add the upgrade to that, else create a new WaveUpgradePair for that wave
                if (specialUpgradeList.Any(vp => vp.Wave == (int)Value))
                {
                    specialUpgradeList.Find(vp => vp.Wave == (int)Value).Upgrades.Add(NewUpgrade);
                }
                else
                {
                    UpgradeManager.WaveUpgradePair pair = new();

                    pair.Wave = (int)Value;
                    pair.Upgrades.Add(NewUpgrade);

                    specialUpgradeList.Add(pair);
                }

                break;
        }

        NewUpgrade = CreateInstance<Upgrade>();
    }
}
