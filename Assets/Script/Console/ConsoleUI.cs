using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleUI : MonoBehaviour
{
    public Text LogText;
    public InputField InputField;
    private StringBuilder _stringBuilder = new StringBuilder();

    private void Awake()
    {
        Console.SetLogCallBack(PrintLog);
    }

    private void OnDisable()
    {
        InputField.text = string.Empty;
        _stringBuilder.Clear();
    }

    private void Update()
    {
        if (InputField == null)
        {
            return;
        }

        if (InputField.text.Length <= 0)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return) == true)
        {
            Console.Execute(InputField.text);
            InputField.text = string.Empty;
        }
    }

    private void PrintLog(string log)
    {
        _stringBuilder.AppendLine(log);

        LogText.text = _stringBuilder.ToString();
    }
}
