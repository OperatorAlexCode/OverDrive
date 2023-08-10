using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // GameObject
    [SerializeField]
    GameObject GameHud;
    [SerializeField]
    GameObject GameOverScreen;
    [SerializeField]
    GameObject UpgradeCard;
    [SerializeField]
    GameObject UpgradeSelection;
    [SerializeField]
    GameObject PauseMenu;
    [SerializeField]
    GameObject SettingsMenu;
    [SerializeField]
    GameObject WinScreen;
    public List<GameObject> PreviouslySelected;

    // Color | Upgrades
    [SerializeField]
    Color ShipUpgrade;
    [SerializeField]
    Color PrimaryUpgrade;
    [SerializeField]
    Color SecondaryUpgrade;
    [SerializeField]
    Color ScatterShotUpgrade;
    [SerializeField]
    Color ChargeShotUpgrade;
    [SerializeField]
    Color MissileUpgrade;
    [SerializeField]
    Color FlakUpgrade;
    [SerializeField]
    Color SpecialUpgrade;

    // Color | Timers
    [SerializeField]
    Color TimerBackground;
    [SerializeField]
    Color TimerBacking;
    [SerializeField]
    Color TimerOverlay;

    // Color | Other
    [SerializeField]
    Color[] HeatBarGradientColors;

    // MenuUI
    public MenuUI CurrentInterface;
    public MenuUI LastInterface;

    // Texture2D
    [SerializeField]
    Texture2D AimCursor;
    [SerializeField]
    Texture2D DefaultCursor;

    // Gradient
    Gradient HeatBarGradient;
    [SerializeField]
    Gradient HealthBarGradient;

    // Slider
    [SerializeField]
    Slider HeatBar;
    [SerializeField]
    Slider HealthBar;
    [SerializeField]
    Slider BossHealthBar;

    // Image
    Image HeatBarFill;
    Image HealthBarFill;
    Image BossHealthBarFill;

    // Timer
    public Timer PrimaryTimer;
    public Timer SecondaryTimer;
    public Timer Ability1Timer;
    public Timer Ability2Timer;

    // Other
    [SerializeField]
    PlayerController Player;
    [SerializeField]
    TextMeshProUGUI HealthText;

    private void Start()
    {
        // Sets values for some ui element in the settings menu
        SettingsMenu.transform.Find("Options").Find(GameObjectNames.ControlerToggle).GetComponent<Toggle>().isOn = GetComponent<GameManager>().UseController;
        SettingsMenu.transform.Find("Options").Find(GameObjectNames.SimpleControlsToggle).GetComponent<Toggle>().isOn = GetComponent<GameManager>().SimpleSteering;
        SettingsMenu.transform.Find("Options").Find(GameObjectNames.RotateSpeedSlider).GetComponent<Slider>().value = Player.RotationalSpeed;

        HeatBarFill = GetSliderFill(HeatBar);
        HealthBarFill = GetSliderFill(HealthBar);
        BossHealthBarFill = GetSliderFill(BossHealthBar);

        // Created the gradient for the Heat bar
        HeatBarGradient = new();

        var colorKey = new GradientColorKey[HeatBarGradientColors.Count()];

        colorKey[0].color = HeatBarGradientColors[0];
        colorKey[0].time = 0;

        for (int x = 1; x < HeatBarGradientColors.Count(); x++)
        {
            colorKey[x].color = HeatBarGradientColors[x];
            colorKey[x].time = (1f / HeatBarGradientColors.Count()) * (float)x;
        }

        HeatBarGradient.SetKeys(colorKey, new GradientAlphaKey[0]);

        // Sets the background and backing for each timer
        List<Timer> timers = new()
        {
            PrimaryTimer,
            SecondaryTimer,
            Ability1Timer,
            Ability2Timer
        };

        foreach (Timer timer in timers)
        {
            timer.SetBackground(TimerBackground);
            timer.SetBacking(TimerBacking);
            timer.SetOverLay(TimerOverlay);
        }

        PrimaryTimer.SetIcon(GetComponent<GameManager>().PlayerLaserColor);
        SecondaryTimer.SetIcon(GetComponent<GameManager>().PlayerTorpedoColor);

        ChangeToAimCursor();
        CurrentInterface = MenuUI.None;
    }

    // Update is called once per frame
    void Update()
    {
        if (Player != null)
        {
            // Sets the max values of the heat and health bars if they aren't the same as for the player
            if (HeatBar.maxValue != Player.MaxHeat)
                HeatBar.maxValue = Player.MaxHeat;

            if (HealthBar.maxValue != Player.MaxHealth)
                HealthBar.maxValue = Player.MaxHealth;

            // Sets the bar's values
            HeatBar.value = Player.Heat;
            HealthBar.value = Player.Health;

            // Changes the color to the health and heat bar to fit their respective gradients
            HeatBarFill.color = HeatBarGradient.Evaluate(Player.Heat/Player.MaxHeat);
            HealthBarFill.color = HealthBarGradient.Evaluate((float)Player.Health/(float)Player.MaxHealth);

            // Wether the player can interact with the controler toggle if 
            SettingsMenu.transform.Find("Options").Find(GameObjectNames.ControlerToggle).GetComponent<Toggle>().interactable = Gamepad.current != null;
        }
    }

    /// <summary>
    /// Show or hide the HUD for the player
    /// </summary>
    /// <param name="toDisplay">True to display, false to hide</param>
    public void DisplayHud(bool toDisplay)
    {
        GameHud.SetActive(toDisplay);
        Cursor.visible = toDisplay;
    }

    /// <summary>
    /// Show the game over screen for the player
    /// </summary>
    public void ShowGameOverScreen()
    {
        GameHud.SetActive(false);
        GameOverScreen.SetActive(true);

        if (GetComponent<GameManager>().UseController)
            SetSelectedUIElement(MenuUI.GameOver);

        else
            ChangeToDefaultCursor();

        CurrentInterface = MenuUI.GameOver;
    }

    public void DisplayUpgrades(List<Upgrade> upgrades)
    {
        foreach (Upgrade upgrade in upgrades)
        {
            GameObject newCard = Instantiate(UpgradeCard);
            newCard.transform.SetParent(UpgradeSelection.transform);

            Color color = new();

            if (upgrade.Changes.Any(c => c.Stat == Stat.WeaponUnlock))
                color = SpecialUpgrade;

            else
                switch (upgrade.Type)
                {
                    case UpgradeType.Ship:
                        color = ShipUpgrade;
                        break;
                    case UpgradeType.Laser:
                        color = PrimaryUpgrade;
                        break;
                    case UpgradeType.Torpedo:
                        color = SecondaryUpgrade;
                        break;
                    case UpgradeType.ScatterShot:
                        color = ScatterShotUpgrade;
                        break;
                    case UpgradeType.ChargedShot:
                        color = ChargeShotUpgrade;
                        break;
                    case UpgradeType.Missile:
                        color = MissileUpgrade;
                        break;
                    case UpgradeType.Flak:
                        color = FlakUpgrade;
                        break;
                }

            newCard.GetComponent<UpgradeCard>().Set(upgrade, color);

            newCard.GetComponent<Button>().onClick.AddListener(ClearDisplayedUpgrades);
            newCard.GetComponent<Button>().onClick.AddListener(GetComponent<GameManager>().StartWave);
        }

        if (GetComponent<GameManager>().UseController)
            SetSelectedUIElement(MenuUI.UpgradeSelection);

        else
            ChangeToDefaultCursor();

        CurrentInterface = MenuUI.UpgradeSelection;
    }

    /// <summary>
    /// Show or hide the pause menu for the player
    /// </summary>
    /// <param name="toDisplay">True to display, false to hide</param>
    public void DisplayPauseMenu(bool toDisplay)
    {
        if (CurrentInterface == MenuUI.UpgradeSelection)
            foreach (Transform card in UpgradeSelection.transform)
                if (card.gameObject.GetComponent<UpgradeCard>().SelectedCard)
                {
                    PreviouslySelected.Add(card.gameObject);
                    break;
                }

        PauseMenu.SetActive(toDisplay);

        if (toDisplay)
        {
            if (GetComponent<GameManager>().UseController)
                SetSelectedUIElement(MenuUI.Pause);

            else
                ChangeToDefaultCursor();

            LastInterface = CurrentInterface;
            CurrentInterface = MenuUI.Pause;
        }
        else
        {
            if (LastInterface == MenuUI.Pause && CurrentInterface == MenuUI.Settings)
            {
                DisplaySettingsMenu(false);
            }

            else
                CurrentInterface = LastInterface;

            if (GetComponent<GameManager>().UseController)
                SelectPreviousSelected();

            else
                ChangeToAimCursor();
        }

    }

    /// <summary>
    /// Show or hide the settings menu for the player
    /// </summary>
    /// <param name="toDisplay">True to display, false to hide</param>
    public void DisplaySettingsMenu(bool toDisplay)
    {
        SettingsMenu.SetActive(toDisplay);

        if (toDisplay)
        {
            if (GetComponent<GameManager>().UseController)
                SetSelectedUIElement(MenuUI.Settings);

            else
                ChangeToDefaultCursor();

            LastInterface = CurrentInterface;
            CurrentInterface = MenuUI.Settings;
        }
        else
        {
            CurrentInterface = LastInterface;

            if (GetComponent<GameManager>().UseController)
                SelectPreviousSelected();

            else
                ChangeToAimCursor();
        }
    }

    public void ClearDisplayedUpgrades()
    {
        foreach (Transform child in UpgradeSelection.transform)
            Destroy(child.gameObject);

        ChangeToAimCursor();

        CurrentInterface = MenuUI.None;
    }

    /// <summary>
    /// Selects the default button or element for a specific menu or interface for when player uses a controler to play
    /// </summary>
    /// <param name="uiInterface">Desired interface</param>
    public void SetSelectedUIElement(MenuUI uiInterface)
    {
        switch (uiInterface)
        {
            case MenuUI.Pause:
                PauseMenu.transform.Find("Buttons").Find("Resume").GetComponent<Button>().Select();
                break;
            case MenuUI.UpgradeSelection:
                UpgradeSelection.transform.GetChild(0).GetComponent<Button>().Select();
                break;
            case MenuUI.GameOver:
                GameOverScreen.transform.Find("Buttons").Find("Retry").GetComponent<Button>().Select();
                break;
            case MenuUI.Settings:
                SettingsMenu.transform.Find(GameObjectNames.PauseMenuOptions).Find(GameObjectNames.RotateSpeedSlider).GetComponent<Slider>().Select();
                break;
        }
    }

    public void SetControlerToggleSelected()
    {
        SettingsMenu.transform.Find("Options").transform.Find(GameObjectNames.ControlerToggle).GetComponent<Toggle>().Select();
    }

    /// <summary>
    /// Selects previously selected ui element of the menu before
    /// </summary>
    void SelectPreviousSelected()
    {
        if (PreviouslySelected.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(PreviouslySelected.Last());

            if (PreviouslySelected.Last().tag == "UpgradeCard")
                CurrentInterface = MenuUI.UpgradeSelection;

            PreviouslySelected.Remove(PreviouslySelected.Last());
        }
    }

    public void AddPreviouslySelected(GameObject previouslySelected)
    {
        PreviouslySelected.Add(previouslySelected);
    }

    void ChangeToAimCursor()
    {
        Cursor.SetCursor(AimCursor, new Vector2(AimCursor.width/2,AimCursor.height/2), CursorMode.Auto);
    }

    void ChangeToDefaultCursor()
    {
        Cursor.SetCursor(DefaultCursor, Vector2.zero, CursorMode.Auto);
    }

    public IEnumerator ActivateBossHealthBar(GameObject boss)
    {
        Drone bossEnemy = boss.GetComponent<Drone>();

        BossHealthBar.maxValue = bossEnemy.GetCurrentHealth();
        BossHealthBar.gameObject.SetActive(true);

        while (boss != null)
        {
            BossHealthBar.value = bossEnemy.GetCurrentHealth();
            BossHealthBarFill.color = HealthBarGradient.Evaluate((float)bossEnemy.GetCurrentHealth() / (float)BossHealthBar.maxValue);
            yield return new WaitForSeconds(.1f);
        }

        BossHealthBar.gameObject.SetActive(false);
    }

    public void ShowBossHealthBar()
    {
        BossHealthBar.gameObject.SetActive(true);
    }

    public void HideBossHealthBar()
    {
        BossHealthBar.gameObject.SetActive(false);
    }

    public void DisplayWinScreen()
    {
        DisplayHud(false);
        WinScreen.SetActive(true);
    }

    Image GetSliderFill(Slider slider)
    {
        return slider.GetComponent<Slider>().fillRect.GetComponent<Image>();
    }
}

public enum MenuUI
{
    None,
    Pause,
    UpgradeSelection,
    GameOver,
    Settings
}
