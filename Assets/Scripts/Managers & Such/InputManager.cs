using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class InputManager : MonoBehaviour
{
    public bool UseController;
    public UnityEvent OnGamepadSwitch;
    public UnityEvent OnKeyboardSwitch;

    [SerializeField] Texture2D AimCursor;
    [SerializeField] Texture2D DefaultCursor;

    // Start is called before the first frame update
    void Start()
    {
        InputSystem.onDeviceChange += (device, change) =>
        {
            switch (change)
            {
                case InputDeviceChange.Added:
                    //if (device is Gamepad)
                    //    SwitchToGamepadControls();
                    Debug.Log($"Device Added:{device}");
                    break;
                case InputDeviceChange.Removed:
                    Debug.Log($"Device Removed:{device}");
                    break;
                case InputDeviceChange.Disconnected:
                    //if (device == Gamepad.current)
                    //    SwitchToKeyboardControls();
                    Debug.Log($"Device Disconnected:{device}");
                    break;
                case InputDeviceChange.Reconnected:
                    //if (device is Gamepad)
                    //    SwitchToGamepadControls();
                    Debug.Log($"Device Reconnected:{device}");
                    break;
            }
        };

        //Debug.Log("Disconnected Devices:");
        //for (int x = 0; x < InputSystem.disconnectedDevices.Count; x++)
        //    Debug.Log(InputSystem.disconnectedDevices[x]);

        //Debug.Log("Connected Devices:");
        //for (int x = 0; x < InputSystem.devices.Count; x++)
        //    Debug.Log(InputSystem.devices[x]);

        if (Gamepad.current != null)
        {
            if (InputSystem.disconnectedDevices.Contains(Gamepad.current))
                SwitchToKeyboardControls();

            else
                SwitchToGamepadControls();
        }

        else
            SwitchToKeyboardControls();

        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        //// Auto switch to controller if one is connected
        //if (PlayerInput.all[0].currentControlScheme == "Keyboar&Mouse" && Gamepad.current != null)
        //    SwitchToGamepadControls();

        //// Auto switch to keyboard if controller has been disconnected
        //else if (PlayerInput.all[0].currentControlScheme == "Gamepad" && Gamepad.current == null)
        //    SwitchToKeyboardControls();

        if (Gamepad.current != null)
        {
            if (PlayerInput.all[0].currentControlScheme == "Keyboard&Mouse" && Gamepad.current.enabled)
                SwitchToGamepadControls();

            else if (PlayerInput.all[0].currentControlScheme == "Gamepad" && !Gamepad.current.enabled)
                SwitchToKeyboardControls();
        }

        else if (PlayerInput.all[0].currentControlScheme == "Gamepad")
            SwitchToKeyboardControls();

        if (FindObjectOfType<InputSystemUIInputModule>() != null)
            FindObjectOfType<InputSystemUIInputModule>().deselectOnBackgroundClick = !(PlayerInput.all[0].currentControlScheme == "Gamepad");
    }

    public void SwitchToGamepadControls()
    {
        Debug.Log("Switching to Gamepad controls");
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        //UIManager ui = GetComponent<UIManager>();

        //if (ui.CurrentInterface == MenuUI.Settings)
        //    ui.SetControlerToggleSelected();

        //else if (EventSystem.current.currentSelectedGameObject != null)
        //    ui.SetSelectedUIElement(ui.CurrentInterface);

        //InputSystem.DisableDevice(Keyboard.current);
        //InputSystem.DisableDevice(Mouse.current);
        //InputSystem.EnableDevice(Gamepad.current);

        Menu currentMenu = FindObjectsOfType<Menu>().FirstOrDefault(m => m.ActiveMenu);

        if (EventSystem.current.currentSelectedGameObject == null && currentMenu != null)
            currentMenu.ForceSelectElement();

        PlayerInput.all[0].SwitchCurrentControlScheme(Gamepad.current);
        UseController = true;

        OnGamepadSwitch.Invoke();
    }

    public void SwitchToKeyboardControls()
    {
        Debug.Log("Switching to Keyboard controls");
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (EventSystem.current.currentSelectedGameObject != null)
            EventSystem.current.SetSelectedGameObject(null);

        ChangeToDefaultCursor();

        GameManager gameManager = FindObjectOfType<GameManager>();

        if (gameManager != null)
        {
            if (gameManager.InWave && !gameManager.GamePaused)
                ChangeToAimCursor();
        }

        /*if (Gamepad.current != null)
            InputSystem.DisableDevice(Gamepad.current);

        InputSystem.EnableDevice(Keyboard.current);
        InputSystem.EnableDevice(Mouse.current);*/

        PlayerInput.all[0].SwitchCurrentControlScheme(Keyboard.current, Mouse.current);
        UseController = false;
        OnKeyboardSwitch.Invoke();
    }

    public void ChangeToAimCursor()
    {
        Cursor.SetCursor(AimCursor, new Vector2(AimCursor.width / 2, AimCursor.height / 2), CursorMode.Auto);
    }

    public void ChangeToDefaultCursor()
    {
        Cursor.SetCursor(DefaultCursor, Vector2.zero, CursorMode.Auto);
    }
}
