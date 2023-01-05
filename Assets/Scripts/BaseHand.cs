using System.Collections;
using UnityEngine;

public abstract class BaseHand : MonoBehaviour
{
    protected System.Diagnostics.Stopwatch _watch = new System.Diagnostics.Stopwatch();

    [HideInInspector] public Coroutine interactiveUpdate;
    
    public abstract void OpenHand();

    protected void StartWatch()
    {
        _watch.Reset();
        _watch.Start();
    }

    protected void StopWatch()
    {
        _watch.Stop();
        Debug.Log($"Time elapsed: {_watch.Elapsed}");
    }

    public void CloseHand()
    {
        CloseHand2();
    }

    protected abstract void CloseHand2();

    public IEnumerator InteractiveUpdate()
    {
        while (true)
        {
            CloseHand();
            yield return new WaitForSeconds(.1f);
        }
    }
}
