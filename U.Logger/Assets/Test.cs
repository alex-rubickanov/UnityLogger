using Rubickanov.Logger;
using UnityEngine;
using Logger = Rubickanov.Logger.Logger;
using LogType = Rubickanov.Logger.LogType;

public class Test : MonoBehaviour
{
    public Logger[] test;

    private void Start()
    {
        foreach (var t in test)
        {
            t.Log(LogLevel.Info, "AHSJ", this, LogType.ConsoleAndScreen);
        }
    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            foreach (var t in test)
            {
                t.Log(LogLevel.Info, "KEY", this, LogType.ConsoleAndScreen);
            }
        }
    }
}