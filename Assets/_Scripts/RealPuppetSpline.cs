using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Thinko
{
    public class RealPuppetSpline : MonoBehaviour
    {
        public Transform RootJoint;
        public List<Transform> Joints = new List<Transform>();
        public bool UseExternalControlPoints;
        public bool ApplySplineRotation;

        private List<Transform> _splineControls;
        private Vector3[] _splineControlsPositions;
        private List<Vector3> _splineCurve = new List<Vector3>();
        private List<int> _inBetweenSamples = new List<int>();
        private List<Transform> _hierarchy = new List<Transform>();

        private void Start()
        {
            _hierarchy = RootJoint.GetComponentsInChildren<Transform>().ToList();
            
            // Create spline control points
            var allJoints = new List<Transform>();
            allJoints.Add(_hierarchy[0]);
            allJoints.Add(_hierarchy[0]);
            foreach (var j in Joints)
            {
                allJoints.Add(j);
            }
            allJoints.Add(_hierarchy[_hierarchy.Count - 1]);
            allJoints.Add(_hierarchy[_hierarchy.Count - 1]);

            if (UseExternalControlPoints)
            {
                _splineControls = new List<Transform>();
                var count = 0;
                foreach (var j in allJoints)
                {
                    var splineControl = new GameObject($"{gameObject.name} - SplineControl{count}").transform;
                    splineControl.position = j.position;
                    splineControl.rotation = j.rotation;
                    _splineControls.Add(splineControl);
                    count++;
                }
            }
            else
            {
                _splineControls = allJoints;
            }
            
            // Create spline control points positions list
            _splineControlsPositions = new Vector3[_splineControls.Count];
            for (var i = 0; i < _splineControlsPositions.Length; i++)
            {
                _splineControlsPositions[i] = Vector3.one * float.MaxValue;
            }

            CalculateSamples();
        }

        private void Update()
        {
            // Get the spline control points positions
            var hasUpdatedControls = false;
            for (var i = 0; i < _splineControls.Count; i++)
            {
                var control = _splineControls[i];
                if (_splineControlsPositions[i] != control.position)
                {
                    _splineControlsPositions[i] = control.position;
                    hasUpdatedControls = true;
                }
            }
            
            // Return if the controls haven't changed
            if(!hasUpdatedControls) return;
            
            // Calculate the spline
            CatmullRom(_splineControlsPositions, out _splineCurve, _inBetweenSamples);
            
            // Calculate the rotations
            var positions = new Vector3[_splineCurve.Count];
            var rotations = new Quaternion[_splineCurve.Count];
            for (var i = 0; i < _splineCurve.Count; i++)
            {
                var outCoord = _splineCurve[i];
                positions[i] = outCoord;
                if (i < _splineCurve.Count - 1)
                {
                    if (i == 0)
                        rotations[i] = _splineControls[0].transform.rotation;
                    else
                    {
                        var blend = i / (float)_splineCurve.Count;
                        var twistDir = Vector3.Lerp(_splineControls[0].transform.forward, _splineControls[_splineControls.Count - 1].transform.forward, blend);
                        rotations[i] = Quaternion.LookRotation(_splineCurve[i] - _splineCurve[i + 1], twistDir) * Quaternion.AngleAxis(90, Vector3.left);
                    }
                }
                else
                {
                    rotations[i] = _splineControls[_splineControls.Count - 2].transform.rotation;
                }
            }
            
            // Apply the spline back to the hierarchy
            for (var i = 0; i < _splineCurve.Count; i++)
            {
                _hierarchy[i].position = positions[i];
                
                if(ApplySplineRotation)
                    _hierarchy[i].rotation = rotations[i];
            }
        }

        private void OnDrawGizmos()
        {
            if (_splineCurve == null) return;

            foreach (var p in _splineCurve)
            {
                Gizmos.DrawWireSphere(p, 0.1f);
            }
        }

        // Calculate the needed inbetween samples to match the hierarchy 
        private void CalculateSamples()
        {
            _inBetweenSamples = new List<int>();

            if (Joints.Count == 0)
            {
                _inBetweenSamples.Add(_hierarchy.Count - 1);
            }
            else
            {
                for (var i = 0; i < Joints.Count; i++)
                {
                    if (i == 0)
                    {
                        var index = _hierarchy.IndexOf(Joints[i]);
                        _inBetweenSamples.Add(index);
                    }
                    else
                    {
                        var index = _hierarchy.IndexOf(Joints[i]) - _hierarchy.IndexOf(Joints[i - 1]);
                        _inBetweenSamples.Add(index);
                    }
                }

                _inBetweenSamples.Add(_hierarchy.Count - _hierarchy.IndexOf(Joints[Joints.Count - 1]) - 1);
            }
        }

        // Control points - minimum 4
        // InBetween points lenght = controlPoints - 3
        private static bool CatmullRom(IReadOnlyList<Vector3> controlPoints, out List<Vector3> splinePoints, IReadOnlyList<int> inBetweenPoints)
        {
            if (controlPoints.Count < 4)
            {
                splinePoints = null;
                return false;
            }

            var results = new List<Vector3>();
            for (var i = 1; i < controlPoints.Count - 2; i++)
            {
                var samples = inBetweenPoints[i - 1];
                for (var j = 0; j < samples; j++)
                    results.Add(PointOnCurve(controlPoints[i - 1], controlPoints[i], controlPoints[i + 1], controlPoints[i + 2], (1f / samples) * j));
            }

            results.Add(controlPoints[controlPoints.Count - 2]);

            splinePoints = results;
            return true;
        }

        private static Vector3 PointOnCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            var result = new Vector3();

            var t0 = ((-t + 2f) * t - 1f) * t * 0.5f;
            var t1 = (((3f * t - 5f) * t) * t + 2f) * 0.5f;
            var t2 = ((-3f * t + 4f) * t + 1f) * t * 0.5f;
            var t3 = ((t - 1f) * t * t) * 0.5f;

            result.x = p0.x * t0 + p1.x * t1 + p2.x * t2 + p3.x * t3;
            result.y = p0.y * t0 + p1.y * t1 + p2.y * t2 + p3.y * t3;
            result.z = p0.z * t0 + p1.z * t1 + p2.z * t2 + p3.z * t3;

            return result;
        }
    }
}