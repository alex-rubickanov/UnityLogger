using UnityEngine;
using Logger = Rubickanov.Logger.Logger;
using LogType = Rubickanov.Logger.LogType;

public class Test : MonoBehaviour
{
    public Logger test;

    private void Start()
    {
        test.Log(LogType.Info, "agagagagagag", this, true, true);
    }
}
