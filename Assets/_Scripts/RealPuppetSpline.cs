using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Thinko
{
    public class RealPuppetSpline : MonoBehaviour
    {
        public Transform RootJoint;
        public List<Transform> Joints = new List<Transform>();

        private List<Transform> _splineControls;
        private List<Vector3> _splineCurve = new List<Vector3>();
        private List<int> _inBetweenSamples = new List<int>();
        private List<Transform> _hierarchy = new List<Transform>();

        private void Start()
        {
            _hierarchy = RootJoint.GetComponentsInChildren<Transform>().ToList();
            
            // Create spline control points
            var joints = new List<Transform>();
            joints.Add(_hierarchy[0]);
            joints.Add(_hierarchy[0]);
            foreach (var j in Joints)
            {
                joints.Add(j);
            }
            joints.Add(_hierarchy[_hierarchy.Count - 1]);
            joints.Add(_hierarchy[_hierarchy.Count - 1]);
            
            _splineControls = new List<Transform>();
            var count = 0;
            foreach (var j in joints)
            {
                var splineControl = new GameObject("SplineControl" + count).transform;
                splineControl.position = j.position;
                splineControl.rotation = j.rotation;
                _splineControls.Add(splineControl);
                count++;
            }
            
            CalculateSamples();
        }

        private void Update()
        {
            // Get the spline control points positions
            var splineControlsPositions = new List<Vector3>();
            foreach (var j in _splineControls)
            {
                splineControlsPositions.Add(j.position);
            }
            
            // Calculate the spline
            CatmullRom(splineControlsPositions, out _splineCurve, _inBetweenSamples);
            
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
//                _hierarchy[i].localRotation = rotations[i];
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