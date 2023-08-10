using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeCard : MonoBehaviour
{
    // Sprite
    [SerializeField]
    Sprite ShipIcon;
    [SerializeField]
    Sprite PrimairyIcon;
    [SerializeField]
    Sprite SecondaryIcon;
    [SerializeField]
    Sprite ScatterShotIcon;
    [SerializeField]
    Sprite ChargedShotIcon;
    [SerializeField]
    Sprite MissileIcon;
    [SerializeField]
    Sprite FlakIcon;

    // Other
    Upgrade Upgrade;
    Vector3 StartScale;
    [SerializeField]
    float ScaleMultiplier;
    public bool SelectedCard;

    private void Update()
    {
        if (GameObject.Find(GameObjectNames.Managers).GetComponent<GameManager>().UseController)
        {
            if (EventSystem.current.currentSelectedGameObject.Equals(gameObject))
                SelectScale(true);
            else
                SelectScale(false);
        }
    }

    public void Set(Upgrade upgrade)
    {
        Upgrade = upgrade;
        gameObject.GetComponent<Button>().onClick.AddListener(UpgradePlayer);
        if (upgrade.Changes.Any(c => c.Stat == Stat.WeaponUnlock))
        {
            switch (upgrade.Type)
            {
                case UpgradeType.ScatterShot:
                    gameObject.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        GameObject.Find("Managers").GetComponent<UpgradeManager>().UnlockWeaponUpgrades(PrimaryWeapon.ScatterShot);
                    });
                    break;
                case UpgradeType.ChargedShot:
                    gameObject.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        GameObject.Find("Managers").GetComponent<UpgradeManager>().UnlockWeaponUpgrades(PrimaryWeapon.ScatterShot);
                    });
                    break;
                case UpgradeType.Missile:
                    gameObject.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        GameObject.Find("Managers").GetComponent<UpgradeManager>().UnlockWeaponUpgrades(SecondaryWeapon.Missile);
                    });
                    break;
                case UpgradeType.Flak:
                    gameObject.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        GameObject.Find("Managers").GetComponent<UpgradeManager>().UnlockWeaponUpgrades(SecondaryWeapon.Flak);
                    });
                    break;
            }

        }

        Image typeIcon = transform.Find("Target Icon").GetComponent<Image>();
        typeIcon.preserveAspect = true;

        GameManager GM = GameObject.Find("Managers").GetComponent<GameManager>();

        switch (upgrade.Type)
        {
            case UpgradeType.Ship:
                typeIcon.sprite = ShipIcon;
                typeIcon.color = GM.PlayerShipColor;
                break;
            case UpgradeType.Laser:
                typeIcon.sprite = PrimairyIcon;
                typeIcon.color = GM.PlayerLaserColor;
                break;
            case UpgradeType.ScatterShot:
                typeIcon.sprite = ScatterShotIcon;
                typeIcon.color = GM.PlayerLaserColor;
                break;
            case UpgradeType.ChargedShot:
                typeIcon.sprite = ChargedShotIcon;
                typeIcon.color = Color.white;
                break;
            case UpgradeType.Torpedo:
                typeIcon.sprite = SecondaryIcon;
                typeIcon.color = Color.white;
                break;
            case UpgradeType.Missile:
                typeIcon.sprite = MissileIcon;
                typeIcon.color = Color.white;
                break;
            case UpgradeType.Flak:
                typeIcon.sprite = FlakIcon;
                typeIcon.color = Color.white;
                break;
        }

        transform.Find("Stat Icon").GetComponent<Image>().sprite = upgrade.Icon;

        Color statIconColor = Color.white;

        if (upgrade.Changes.Any(c => c.Stat == Stat.WeaponUnlock))
            statIconColor = new Color(0, 0, 0, 0);

        else if (upgrade.Type == UpgradeType.ScatterShot)
            statIconColor = GM.PlayerLaserColor;

        transform.Find("Stat Icon").GetComponent<Image>().color = statIconColor;


        //StartScale = transform.localScale;
        transform.localScale = Vector3.one;
        StartScale = Vector3.one;
    }

    public void Set(Upgrade upgrade, Color cardColor)
    {
        GetComponent<Image>().color = cardColor;
        Set(upgrade);
    }

    public void UpgradePlayer()
    {
        GameObject.Find(GameObjectNames.Player).GetComponent<PlayerController>().Upgrade(Upgrade);
        GameObject.Find(GameObjectNames.Player).GetComponent<PlayerController>().EnableDisableAutoStop(false);
    }

    /// <summary>
    /// Change the scale of card if cursor hovers over card or it is currently selected
    /// </summary>
    public void SelectScale()
    {
        if (GameObject.Find(GameObjectNames.Managers).GetComponent<UIManager>().CurrentInterface == MenuUI.UpgradeSelection)
        {
            if (transform.localScale == StartScale)
                transform.localScale = StartScale * ScaleMultiplier;
            else
                transform.localScale = StartScale;
        }
    }

    /// <summary>
    /// Change the scale of card if cursor hovers over card or it is currently selected
    /// </summary>
    public void SelectScale(bool select)
    {
        if (GameObject.Find(GameObjectNames.Managers).GetComponent<UIManager>().CurrentInterface == MenuUI.UpgradeSelection)
        {
            if (select)
                transform.localScale = StartScale * ScaleMultiplier;
            else
                transform.localScale = StartScale;

            SelectedCard = select;
        }
    }
}