using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsoleRoot : MonoBehaviour
{
    public GameObject ConsoleUI;

    private void Awake()
    {
        ConsoleUI.SetActive(false);
    } 

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote) == true)
        {
            ConsoleUI.SetActive(!ConsoleUI.activeSelf);
        }
    }
}
