using UnityEngine;
using Rubickanov.Logger;

public class Player : MonoBehaviour
{
    [SerializeField] private RubiLogger rubiLogger;


    private void Start()
    {
        for (int i = 0; i < 10000; i++)
        {
            rubiLogger.Log(LogLevel.Info, i, this, LogOutput.ConsoleAndFile);
        }
    }
}


public class Testik
{
    public void Start()
    {
        RubiLoggerStatic.Log(LogLevel.Info, "Im GAY", "Testik", LogOutput.All, "KIKI");
    }
}