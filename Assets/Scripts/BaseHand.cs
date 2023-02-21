using System;
using System.Collections;
using UnityEngine;

public abstract class BaseHand : MonoBehaviour
{
    [SerializeField] protected HandPoserBase Poser;
    protected System.Diagnostics.Stopwatch _watch = new System.Diagnostics.Stopwatch();

    [HideInInspector] public Coroutine interactiveUpdate;

    public void OpenHand()
    {
        Poser.Squish = 0;
    }

    protected void StartWatch()
    {
        _watch.Reset();
        _watch.Start();
    }

    protected void StopWatch(string context)
    {
        _watch.Stop();
        Debug.Log($"Time elapsed in {context}: {_watch.Elapsed}");
    }

    public void CloseHand()
    {
        CloseHand2();
    }

    protected abstract void CloseHand2();

    public IEnumerator InteractiveUpdate()
    {
        var weof = new WaitForSeconds(.05f);
        while (true)
        {
            CloseHand();
            yield return weof;
        }
    }
}
