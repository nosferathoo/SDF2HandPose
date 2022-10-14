using UnityEngine;

public class PhysicsHand : BaseHand
{
    [SerializeField] private Finger[] fingers;
    [SerializeField] private float alphaStep = 0.01f;
    [SerializeField] private float tipRadius = 0.01f;
    [SerializeField] private LayerMask layerMask;

    private Collider[] _colliders = new Collider[2];
    private void OnValidate()
    {
        fingers = GetComponentsInChildren<Finger>();
    }

    public override void OpenHand()
    {
        foreach (var finger in fingers)
        {  
            finger.Squish = 0;
        }
    }

    protected override void CloseHand2()
    {
        foreach (var finger in fingers)
        {
            for (var alpha = 0f; alpha < 1f; alpha += alphaStep)
            {
                finger.Squish = alpha;

                if (Physics.OverlapSphereNonAlloc(finger.Pad.position, tipRadius, _colliders, layerMask) > 0)
                {
                    break;
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        foreach (var finger in fingers)
        {
            Gizmos.DrawWireSphere(finger.Pad.position, tipRadius);
        }
    }
}