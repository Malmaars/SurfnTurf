using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.InputSystem;

public class CommandConsole : MonoBehaviour
{
    public GameObject consoleUI;
    public TMP_InputField inputField;

    private bool isConsoleOpen = false;
    void Start()
    {
        consoleUI.SetActive(false);
        CommandRegistry.RegisterAllCommands(); // Register once
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Slash))
        {
            ToggleConsole();
        }

        if (isConsoleOpen && Input.GetKeyDown(KeyCode.Return))
        {
            string command = inputField.text;
            inputField.text = "";
            ToggleConsole();
            CommandRegistry.Execute(command);
        }
    }

    void ToggleConsole()
    {
        isConsoleOpen = !isConsoleOpen;
        consoleUI.SetActive(isConsoleOpen);

        if (isConsoleOpen)
        {
            EventSystem.current.SetSelectedGameObject(inputField.gameObject);
            inputField.ActivateInputField();
        }
    }
}

