using UnityEngine;
using Rubickanov.Logger;

public class Test : MonoBehaviour
{
    public RubiLogger test;
    public string message;
    public int inputKey;
    
    public LogLevel LogLevel;

    private void Start()
    {
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(inputKey.ToString()))
        {
            test.Log(LogLevel, message, this, LogOutput.ConsoleAndFile);
        }
    }
}