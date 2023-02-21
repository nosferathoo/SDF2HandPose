using System;
using UnityEngine;

public class FingerPart : FingerBase
{
    [SerializeField] private int phase=-1;
    [SerializeField] private bool direction;

    public int Phase => phase;

    private void OnValidate()
    {
        if (Phase == -1)
        {
            phase = GetComponentsInParent<FingerPart>().Length-1;
        }
    }
}