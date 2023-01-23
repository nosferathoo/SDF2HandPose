using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class SDFMagnet : MonoBehaviour
    {
        [SerializeField] private SDFUpdater sdfUpdater;
        [SerializeField] private SDFSampler sdfSampler;
        [SerializeField] private Transform[] pointsInHand;
        [SerializeField] private float maxStep = 0.1f;
        [SerializeField] private float minDistToStop = 0.05f;
        public void Start()
        {
            StartCoroutine(PrepareSurfacePoints());
        }

        private IEnumerator PrepareSurfacePoints()
        {
            yield return new WaitForSeconds(1f);
            var vertexes = pointsInHand.Select(t => sdfUpdater.WorldToTexPos(t.position)).ToList();

            sdfSampler.SetupSampler(sdfUpdater.renderTexture, vertexes.Count, 4);
            sdfSampler.SetSamplerData(vertexes);
        }

        public void Use()
        {
            var output = sdfSampler.RunSampler();
            AsyncGPUReadback.Request(output, request =>
            {
                if (!Application.isPlaying) // FIX for GPU callback running after stopping app in unity editor
                    return;
            
                var resultArr = request.GetData<Vector4>();

                var minDist = resultArr.Average(f => f.w);
                var normal = resultArr.Aggregate(Vector3.zero, (a,b)=> a + (Vector3)b) / resultArr.Length;
                Debug.Log($"{minDist} , {normal}");
                if (minDist < minDistToStop)
                {
                    Debug.Log("Minimal distance reached");
                    return;
                }

                minDist = Mathf.Min(maxStep, minDist);
                if (normal.sqrMagnitude > 0)
                {
                    normal = transform.TransformDirection(normal);
                    transform.rotation = Quaternion.Lerp(transform.rotation,
                        Quaternion.LookRotation(transform.forward, normal),
                        .5f);
                }
                transform.Translate(-Vector3.up * minDist * sdfUpdater.transform.localScale.x, Space.Self);
            });
        }
        
        private void OnDrawGizmosSelected()
        {
            for (var i=0;i<pointsInHand.Length;++i)
            {
                Gizmos.DrawLine(pointsInHand[i].position,pointsInHand[(i+1)%pointsInHand.Length].position);
            }
        }

    }
