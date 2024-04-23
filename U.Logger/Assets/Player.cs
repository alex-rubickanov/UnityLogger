using UnityEngine;
using Rubickanov.Logger;

public class Player : MonoBehaviour
{
    [SerializeField] private RubiLogger rubiLogger;


    private void Start()
    {
        //rubiLogger.Log(LogLevel.Info, "Player started!", this, LogOutput.ConsoleAndFile);

        Testik testik = new Testik();
        testik.Start();
        
        rubiLogger.Log(LogLevel.Error, "Keks", this, LogOutput.All);
    }
}


public class Testik
{
    public void Start()
    {
        RubiLoggerStatic.Log(LogLevel.Info, "Im GAY", "Testik", LogOutput.All, "KIKI");
    }
}