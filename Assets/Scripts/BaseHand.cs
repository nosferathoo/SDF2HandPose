using UnityEngine;

public abstract class BaseHand : MonoBehaviour
{
    private System.Diagnostics.Stopwatch _watch = new System.Diagnostics.Stopwatch();
    
    public abstract void OpenHand();

    public void CloseHand()
    {
        _watch.Reset();
        _watch.Start();
        CloseHand2();
        _watch.Stop();
        Debug.Log($"Time elapsed: {_watch.Elapsed}");
    }

    protected abstract void CloseHand2();
}
