//hotfix
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class Oriantation : MonoBehaviour
{
    [SerializeField] TMP_InputField input;
    // Start is called before the first frame update
    void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;

        input.onSelect.AddListener(showKeyboard);
    }
    void showKeyboard(string str)
    {
        TouchScreenKeyboard.Open("");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
