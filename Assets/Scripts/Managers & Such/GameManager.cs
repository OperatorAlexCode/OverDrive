using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Pathfinding;
using UnityEngine.Windows;

public class GameManager : MonoBehaviour
{
    // Color
    public Color PlayerShipColor;
    public Color PlayerLaserColor;
    public Color PlayerTorpedoColor;
    public Color EnemyProjectilesColor;

    // Bool
    bool InWave;
    public bool UseController;
    public bool GamePaused;
    public bool SimpleSteering;
    [SerializeField] bool BeginWave;
    bool[] PlayerVars = new bool[4];

    //// String
    //public string UseControllerKey = "UseController";
    //public string SimpleSteeringKey = "SimpleSteering";
    //public string RotationalSpeedKey = "RotateSpeed";

    // Other
    public int Wave;
    PlayerController Player;
    InputAction PauseAction;
    public PlayerInputActions PlayerControls;
    public Rect PlayArea;

    /*private void OnEnable()
    {
        PauseAction = PlayerControls.Player.Pause;
        PauseAction.Enable();

        PauseAction.performed += (InputAction.CallbackContext context) => PauseUnpauseGame();
    }

    private void OnDisable()
    {
        PauseAction.Disable();
    }

    private void Awake()
    {
        PlayerControls = new();
    }*/

    // Start is called before the first frame update
    void Start()
    {
        // Finds and sets the players color
        Player = GameObject.Find(GameObjectNames.Player).GetComponent<PlayerController>();
        Player.SetColor(PlayerShipColor);

        // Load settings from playerprefs
        if (PlayerPrefs.HasKey(PlayerPrefkeys.UseControllerKey))
            UseController = Convert.ToBoolean(PlayerPrefs.GetInt(PlayerPrefkeys.UseControllerKey));

        if (PlayerPrefs.HasKey(PlayerPrefkeys.SimpleSteeringKey))
            SimpleSteering = Convert.ToBoolean(PlayerPrefs.GetInt(PlayerPrefkeys.SimpleSteeringKey));

        if (PlayerPrefs.HasKey(PlayerPrefkeys.RotationalSpeedKey))
            Player.RotationalSpeed = PlayerPrefs.GetFloat(PlayerPrefkeys.RotationalSpeedKey);

        // If the player want to use a controller and a controller is plugged in then hide the cursor, disable mouse & keyboard and enable controller
        if (UseController & Gamepad.current != null)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            //InputSystem.DisableDevice(Keyboard.current);
            //InputSystem.DisableDevice(Mouse.current);
            //InputSystem.EnableDevice(Gamepad.current);

            PlayerInput.all[0].SwitchCurrentControlScheme(Gamepad.current);
        }

        // Otherwise disable controller if there is one connected
        else if (Gamepad.current != null)
        {
            //InputSystem.DisableDevice(Gamepad.current);
            //InputSystem.EnableDevice(Keyboard.current);
            //InputSystem.EnableDevice(Mouse.current);
            PlayerInput.all[0].SwitchCurrentControlScheme(Keyboard.current, Mouse.current);
            UseController = false;
        }

        // Spawn in debris inside the play area
        GetComponent<DebrisSpawner>().SpawnInArea(30, PlayArea);

        GetComponent<SoundManager>().UpdateVolumeAll();

        if (BeginWave)
            StartWave();
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerInput.all[0].currentControlScheme == "Gamepad" && Gamepad.current == null)
            SwitchToKeyboardControls();

        /*else if (PlayerInput.all[0].currentControlScheme == "Keyboar&Mouse" && Gamepad.current != null)
            SwitchToGamepadControls();*/

        if (IsWaveDone())
        {
            InWave = false;

            if (gameObject.GetComponent<EnemySpawner>().LastWave(Wave))
                GameWin();

            else
            {
                List<Upgrade> upgrades = GetComponent<UpgradeManager>().GetUpgradeSet(4);
                GetComponent<UIManager>().DisplayUpgrades(upgrades);
                Player.EnableDisableAutoStop(true);
                Player.EnableDisableWeapons(true);
                Player.EnableDisableAiming(true);
            }
        }
    }

    public void StartWave()
    {
        gameObject.GetComponent<EnemySpawner>().StartWave(++Wave);
        InWave = true;
        Player.EnableDisableAutoStop(false);
        Player.EnableDisableWeapons(false);
        Player.EnableDisableAiming(false);
    }

    bool IsWaveDone()
    {
        return InWave && !GetComponent<EnemySpawner>().EnemiesStillLeft() && gameObject.GetComponent<EnemySpawner>().TotalEnemiesToSpawn == 0;
    }

    public void SwitchToGamepadControls()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        UIManager ui = GetComponent<UIManager>();

        if (ui.CurrentInterface == MenuUI.Settings)
            ui.SetControlerToggleSelected();

        else if (EventSystem.current.currentSelectedGameObject != null)
            ui.SetSelectedUIElement(ui.CurrentInterface);

        //InputSystem.DisableDevice(Keyboard.current);
        //InputSystem.DisableDevice(Mouse.current);
        //InputSystem.EnableDevice(Gamepad.current);

        PlayerInput.all[0].SwitchCurrentControlScheme(Gamepad.current);

        UseController = true;
    }

    public void SwitchToKeyboardControls()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (EventSystem.current.currentSelectedGameObject != null)
            EventSystem.current.SetSelectedGameObject(null);

        /*if (Gamepad.current != null)
            InputSystem.DisableDevice(Gamepad.current);

        InputSystem.EnableDevice(Keyboard.current);
        InputSystem.EnableDevice(Mouse.current);*/

        PlayerInput.all[0].SwitchCurrentControlScheme(Keyboard.current, Mouse.current);

        UseController = false;
    }

    public void PauseUnpauseGame()
    {
        UIManager uiMngr = GetComponent<UIManager>();
        if (uiMngr.CurrentInterface != MenuUI.GameOver || !(uiMngr.CurrentInterface == MenuUI.Settings && uiMngr.LastInterface == MenuUI.GameOver))
        {
            if (!GamePaused)
                Time.timeScale = 0;
            else
                Time.timeScale = 1;

            uiMngr.DisplayPauseMenu(!GamePaused);

            //if (uiMngr.LastInterface == MenuUI.None)
            //{
            //    Player.EnableDisableHeat(!GamePaused);
            //    Player.EnableDisableMovement(!GamePaused);
            //    Player.EnableDisableWeapons(!GamePaused);
            //    Player.EnableDisableAiming(!GamePaused);
            //}

            if (GamePaused)
            {
                Player.DisableHeat = PlayerVars[0];
                Player.DisableMovement = PlayerVars[1];
                Player.DisableWeapons = PlayerVars[2];
                Player.DisableAiming = PlayerVars[3];
            }
            else
            {
                PlayerVars[0] = Player.DisableHeat;
                PlayerVars[1] = Player.DisableMovement;
                PlayerVars[2] = Player.DisableWeapons;
                PlayerVars[3] = Player.DisableAiming;
            }

            GamePaused = !GamePaused;
        }
    }

    public void ControlerToggle(bool useGamepad)
    {
        if (useGamepad)
            SwitchToGamepadControls();
        else
            SwitchToKeyboardControls();

        PlayerPrefs.SetInt(PlayerPrefkeys.UseControllerKey, Convert.ToInt32(UseController));
        PlayerPrefs.Save();
    }

    public void SimpleSteeringToggle(bool useSimpleSteering)
    {
        SimpleSteering = useSimpleSteering;
    }

    public void SetPlayerRotationSpeed(float newValue)
    {
        if (GameObject.Find(GameObjectNames.Player) != null)
            GameObject.Find(GameObjectNames.Player).GetComponent<PlayerController>().RotationalSpeed = newValue;

        PlayerPrefs.SetFloat(PlayerPrefkeys.RotationalSpeedKey, newValue);
        PlayerPrefs.Save();
    }

    public void ExitGame()
    {
        PlayerPrefs.SetInt(PlayerPrefkeys.UseControllerKey, Convert.ToInt32(UseController));
        PlayerPrefs.SetInt(PlayerPrefkeys.SimpleSteeringKey, Convert.ToInt32(SimpleSteering));
        PlayerPrefs.SetFloat(PlayerPrefkeys.RotationalSpeedKey, Player.RotationalSpeed);
        PlayerPrefs.Save();

        GetComponent<SceneController>().ExitGame();
    }

    public void GameWin()
    {
        Player.Disable(true);
        Player.enabled = false;
        GetComponent<DebrisSpawner>().ClearAllDebris();
        GetComponent<UIManager>().DisplayWinScreen();
    }

    public void DeviceLost(PlayerInput input)
    {
        input.SwitchCurrentControlScheme(Keyboard.current,Mouse.current);
    }

    public void DeviceRegained(PlayerInput input)
    {
        input.SwitchCurrentControlScheme(Gamepad.current);
    }

    public void ControlsChanged(PlayerInput input)
    {
        if (Gamepad.current != null)
            SwitchToGamepadControls();

        else
            SwitchToKeyboardControls();
    }
}
