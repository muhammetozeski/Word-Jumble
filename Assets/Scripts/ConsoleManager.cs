using System;
using TMPro;
using UnityEngine;

public class ConsoleManager : MonoBehaviour
{
    [SerializeField] TMP_InputField Input;
    [SerializeField] TextMeshProUGUI Output;

    [SerializeField] GameObject CLIGame;
    ICLIGame Game;

    public bool DebugMode = false;

    public void Print(object Text)
    {
        const string CLILineHeader = "> ";
        Output.text += CLILineHeader + Text.ToString() + "\n";
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
        Input.ActivateInputField();

        Game = CLIGame.GetComponent<ICLIGame>();
        Input.onSubmit.AddListener(OnInputSubmit);

        Game.OnStart();
    }
    private void OnInputSubmit(string InputText)
    {
        if (string.IsNullOrEmpty(InputText)) return;

        Print(InputText);
        Input.text = "";

        Action inputFunction;
        if (Game.InputFunctions.TryGetValue(InputText, out inputFunction))
            inputFunction();
        else
            Game.OnCommandNotFound(InputText);

        Input.ActivateInputField();
    }
}
