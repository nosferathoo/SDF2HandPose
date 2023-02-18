using UnityEngine;

[ExecuteAlways]
public class PhysicsHand : BaseHand
{
    
    [SerializeField] private float alphaStep = 0.01f;
    [SerializeField] private float tipRadius = 0.01f;
    [SerializeField] private LayerMask layerMask;

    private Collider[] _colliders = new Collider[2];
    
    protected override void CloseHand2()
    {
        StartWatch();
        foreach (var finger in Poser.Fingers)
        {
            for (var alpha = 0f; alpha < 1f; alpha += alphaStep)
            {
                finger.Squish = alpha;

                if (Physics.OverlapSphereNonAlloc(finger.Tip.position, tipRadius, _colliders, layerMask) > 0)
                {
                    break;
                }
            }
        }

        StopWatch("physics finger posing");
    }

    private void OnDrawGizmosSelected()
    {
        foreach (var finger in Poser.Fingers)
        {
            Gizmos.DrawWireSphere(finger.Tip.position, tipRadius);
        }
    }
}