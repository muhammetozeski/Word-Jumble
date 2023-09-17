using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConsoleManager : MonoBehaviour
{
    [SerializeField] TMP_InputField ConsoleInput;
    [SerializeField] TextMeshProUGUI Output;

    [SerializeField] GameObject CLIGame;
    ICLIGame Game;

    public bool DebugMode = false;

    List<string> writeToConsole = new List<string>();
    private bool isPrinting = false;
    public void Print(object text, float delay)
    {
        writeToConsole.Add(text.ToString());

        if (!isPrinting)
        {
            StartCoroutine(StartPrinting(delay));
        }
    }
    private IEnumerator StartPrinting(float delay)
    {
        isPrinting = true;

        while (writeToConsole.Count > 0)
        {
            printToConsole(writeToConsole[0]);
            writeToConsole.RemoveAt(0);
            yield return new WaitForSeconds(delay);
        }

        isPrinting = false;
    }
    void printToConsole(string text)
    {
        Output.text += "> " + text + "\n";
    }

    public void DPrint(object Text)
    {
        string CLILineHeader = ">>>>>>>>>>>>>>>>>>>>>> ";
        if (DebugMode)
            Output.text += CLILineHeader + Text.ToString() + "\n";
    }

    public void ClearConsole()
    {
        Output.text = "";
    }

    public string GetConsoleLog()
    {
        return Output.text;
    }

    private void Start()
    {
        ConsoleInput.ActivateInputField();
        

        Game = CLIGame.GetComponent<ICLIGame>();
        ConsoleInput.onSubmit.AddListener(OnInputSubmit);
        ConsoleInput.onSelect.AddListener(txt =>
        {
            TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
            ConsoleInput.ActivateInputField();
        });

        Game.OnStart();
    }
    private void OnInputSubmit(string InputText)
    {
        if (string.IsNullOrEmpty(InputText)) return;

        printToConsole(InputText);

        ConsoleInput.text = "";

        Action inputFunction;
        if (Game.InputFunctions.TryGetValue(InputText, out inputFunction))
            inputFunction();
        else
            Game.OnCommandNotFound(InputText);

        ConsoleInput.ActivateInputField();
    }
}
