using System;
using UnityEngine;
using Rubickanov.Logger;

public class Player : MonoBehaviour
{
    [SerializeField] private RubiLogger rubiLogger;
    

    private void Start()
    {
        rubiLogger.Log(LogLevel.Info, "Player started!", this, LogOutput.ConsoleAndFile);
    }
}


