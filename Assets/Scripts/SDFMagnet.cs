using System.Collections;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class SDFMagnet : MonoBehaviour
    {
        [SerializeField] private SDFUpdater sdfUpdater;
        [SerializeField] private SDFSampler sdfSampler;
        [SerializeField] private Transform[] pointsInHand;
        [SerializeField] private float maxStep = 0.1f;
        [SerializeField] private float marginFromObject = 0.1f;
        [SerializeField] private float minDistToStop = 0.01f;
        [SerializeField] private int maxStepCount = 10;
        [SerializeField] private BaseHand grasper;

        // reach to grasp animation stuff
        private int _currentStep = 0;
        private Vector3 _originalPosition;
        private Quaternion _originalRotation;
        private UnityEvent _finishedApproach = new UnityEvent();
        private UnityEvent _finishedStep = new UnityEvent();
        private bool _isRepeating = false;
        
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

        public void Step()
        {
            var output = sdfSampler.RunSampler();
            AsyncGPUReadback.Request(output, request =>
            {
                if (!Application.isPlaying) // FIX for GPU callback running after stopping app in unity editor
                    return;
            
                var resultArr = request.GetData<Vector4>();

                var minDist = resultArr.Average(f => f.w - marginFromObject);
                var normal = resultArr.Aggregate(Vector3.zero, (a,b)=> a + (Vector3)b) / resultArr.Length;
                Debug.Log($"{minDist} , {normal}");
                if (Mathf.Abs(minDist) <= minDistToStop)
                {
                    Debug.Log("Minimal distance reached");
                    _finishedApproach.Invoke();
                    return;
                }

                minDist = Mathf.Min(maxStep, minDist);
                if (normal.sqrMagnitude > 0)
                {
                    normal = transform.TransformDirection(normal);
                    //var tmp = transform.right;
                    //transform.up = normal;
                    //transform.right = tmp;
                    transform.rotation = Quaternion.Lerp(transform.rotation,
                        Quaternion.LookRotation(normal,-transform.forward)*Quaternion.Euler(90,0,0),
                        .5f);
                }
                transform.Translate(-Vector3.up * minDist * sdfUpdater.transform.localScale.x, Space.Self);
                _finishedStep.Invoke();
            });
        }
        
        private void OnDrawGizmosSelected()
        {
            for (var i=0;i<pointsInHand.Length;++i)
            {
                Gizmos.DrawLine(pointsInHand[i].position,pointsInHand[(i+1)%pointsInHand.Length].position);
            }
        }

        public void ApproachAndGrasp()
        {
            grasper.OpenHand();
            _currentStep = 0;
            _finishedApproach.AddListener(OnFinishedApproach);
            _finishedStep.AddListener(OnFinishedStep);
            var t = transform;
            _originalPosition = t.position;
            _originalRotation = t.rotation;
            
            Step();
        }

        public void ApproachAndGraspAndRepeat()
        {
            if (_isRepeating)
            {
                _isRepeating = false;
                RemoveListeners();
                return;
            }

            _isRepeating = true;
            ApproachAndGrasp();
        }

        private IEnumerator WaitAndStep()
        {
            yield return new WaitForEndOfFrame();
            Step();
        }

        private void OnFinishedStep()
        {
            _currentStep++;
            if (_currentStep >= maxStepCount)
            {
                RevertToOriginalPosition();
                if (!_isRepeating)
                {
                    RemoveListeners();
                    return;
                }
            }

            StartCoroutine(WaitAndStep());
        }

        private IEnumerator Repeat()
        {
            yield return new WaitForSeconds(1);
            _currentStep = 0;
            grasper.OpenHand();
            yield return new WaitForEndOfFrame();
            RevertToOriginalPosition();
            yield return new WaitForEndOfFrame();
            Step();
        }
        private void OnFinishedApproach()
        {
            grasper.CloseHand();
            if (_isRepeating)
            {
                StartCoroutine(Repeat());
            }
            else
            {
                RemoveListeners();
            }
        }

        private void RevertToOriginalPosition()
        {
            var t = transform;
            t.position = _originalPosition;
            t.rotation = _originalRotation;
        }

        private void RemoveListeners()
        {
            _finishedApproach.RemoveListener(OnFinishedApproach);
            _finishedStep.RemoveListener(OnFinishedStep);            
        }
    }
