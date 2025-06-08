using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Linq;

public class Menu : MonoBehaviour
{
    public Selectable DefaultSelectedElement;
    [HideInInspector] public GameObject LastSelectedElement;
    [HideInInspector] public Menu PreviousMenu;
    public bool ActiveMenu;
    [SerializeField] bool HideOnSwitch = true;
    public UnityEvent OnSwitchTo;

    private void Start()
    {
        if (ActiveMenu)
            SwitchTo();
    }

    private void Update()
    {
        if (ActiveMenu)
            if (gameObject.GetComponentsInChildren<Selectable>().Any(e => EventSystem.current.currentSelectedGameObject == e.gameObject))
                LastSelectedElement = EventSystem.current.currentSelectedGameObject;
    }

    public void SwitchTo(Menu oldMenu = null)
    {
        if (oldMenu != null)
            oldMenu.SwitchFrom();

        PreviousMenu = oldMenu;
        gameObject.SetActive(true);

        if (FindObjectOfType<InputManager>().UseController)
        {
            //EventSystem.current.SetSelectedGameObject(LastSelectedElement != null ? LastSelectedElement : (DefaultSelectedElement != null ? DefaultSelectedElement.gameObject : null));

            //if (EventSystem.current.currentSelectedGameObject == null)
            //    ForceSelectElement();

            if (LastSelectedElement != null)
                EventSystem.current.SetSelectedGameObject(LastSelectedElement);

            else if (DefaultSelectedElement != null)
                EventSystem.current.SetSelectedGameObject(DefaultSelectedElement.gameObject);

            else
                ForceSelectElement();
        }

        ActiveMenu = true;
        OnSwitchTo.Invoke();
    }

    public void SwitchFrom()
    {
        ActiveMenu = false;
        LastSelectedElement = EventSystem.current.currentSelectedGameObject;

        if (HideOnSwitch)
            gameObject.SetActive(false);
    }

    public void GoBack()
    {
        LastSelectedElement = null;
        ActiveMenu = false;
        PreviousMenu.SwitchTo();
        gameObject.SetActive(false);
    }

    public void ForceHide()
    {
        LastSelectedElement = null;
        ActiveMenu = false;
        gameObject.SetActive(false);
    }

    public void ForceSelectElement()
    {
        if (gameObject.GetComponentsInChildren<Selectable>().Count() > 0)
            EventSystem.current.SetSelectedGameObject(gameObject.GetComponentsInChildren<Selectable>().First().gameObject);
    }

    public void ForceSelectElement(GameObject objectToSelect)
    {
        EventSystem.current.SetSelectedGameObject(objectToSelect);
    }
}
