﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets
{
    public enum CustomModeClassification
    {
        Default,
        Circle,
        Xor,
        Cross
    }
    
    public class LinearClassification : MonoBehaviour
    {
        private int _dimensions = 2;
        
        private GameObject[] _spheresPlan;
        private GameObject[] _spheres;
        private List<double> _inputs;
        private IntPtr _model;
        private int[] _expectedSigns;
        private float _centerZ;
        private float _centerX;
        private float _maxDistanceToAxis;

        [SerializeField]
        private int _numberOfIterations = 1;

        public int NumberOfIterations
        {
            get => _numberOfIterations;
            set => _numberOfIterations = value;
        }

        [SerializeField]
        private CustomModeClassification _mode = CustomModeClassification.Default;

        public CustomModeClassification Mode
        {
            get => _mode;
            set => _mode = value;
        }

        [SerializeField]
        private double _learnStep = 0.001;

        public double LearnStep
        {
            get => _learnStep;
            set => _learnStep = value;
        }

        private void Start()
        {
            _dimensions = _mode == CustomModeClassification.Xor || _mode == CustomModeClassification.Cross ? 1 : 2;
            _spheresPlan = GameObject.FindGameObjectsWithTag("plan");
            _spheres = GameObject.FindGameObjectsWithTag("sphere");

            Debug.Log($"Sphere number : {_spheres.Length}");
            Debug.Log($"PlanSphere number : {_spheresPlan.Length}");
            Debug.Log("Starting to call library for a LinearClassification");

            _model = ClassificationLibrary.createModel(_dimensions);

            if (Mode == CustomModeClassification.Circle)
            {
                var allPointsWith1 = _spheres.Where(sp => sp.transform.position.y > 0).ToList();
                float totalX = 0, totalZ = 0;
                foreach (var p in allPointsWith1)
                {
                    var position = p.transform.position;
                    totalX += position.x;
                    totalZ += position.z;
                }
                _centerX = totalX / allPointsWith1.Count;
                _centerZ = totalZ / allPointsWith1.Count;
            }

            if (Mode == CustomModeClassification.Xor || Mode == CustomModeClassification.Cross)
            {
                var allPoints = _spheresPlan;
                float totalX = 0, totalZ = 0;
                foreach (var p in allPoints)
                {
                    var position = p.transform.position;
                    totalX += position.x;
                    totalZ += position.z;
                }
                _centerX = totalX / allPoints.Length;
                _centerZ = totalZ / allPoints.Length;
            }

            if (Mode == CustomModeClassification.Cross)
            {
                var allPointsWith1 = _spheres.Where(sp => sp.transform.position.y > 0).ToList();
                foreach (var p in allPointsWith1)
                {
                    var position = p.transform.position;

                    var posX = Math.Abs(position.x - _centerX);
                    var posZ = Math.Abs(position.z - _centerZ);

                    var min = Math.Min(posX, posZ);
                    var distance = Math.Max(_maxDistanceToAxis, min);
                    if (distance > _maxDistanceToAxis)
                    {
                        _maxDistanceToAxis = distance;
                    }
                }
            }
            
            Debug.Log("Found center at X = "+_centerX+" | Z = "+_centerZ);
            
            _expectedSigns = _spheres.Select(sp => sp.transform.position.y < 0 ? -1 : 1).ToArray();
            _inputs = new List<double>();
            foreach (var sphere in _spheres)
            {
                var position = sphere.transform.position;
                if (Mode == CustomModeClassification.Xor)
                {
                    _inputs.Add(MapPositionX(position.x) * MapPositionZ(position.z));
                }
                else if (Mode == CustomModeClassification.Cross)
                {
                    var posX = Math.Abs(position.x - _centerX);
                    var posZ = Math.Abs(position.z - _centerZ);
                    
                    var min = Math.Min(posX, posZ);
                    var abs = Math.Abs(min) - _maxDistanceToAxis;
                    _inputs.Add(abs);
                }
                else
                {
                    _inputs.Add(MapPositionX(position.x));
                    _inputs.Add(MapPositionZ(position.z));
                }
            }
        }

        private double MapPositionX(double x)
        {
            switch (Mode)
            {
                case CustomModeClassification.Xor:
                    return x - _centerX;
                case CustomModeClassification.Default:
                    return x;
                case CustomModeClassification.Circle:
                    return Math.Pow(x - _centerX, 2);
                default:
                    return x;
            }
        }
        
        private double MapPositionZ(double z)
        {
            switch (Mode)
            {
                case CustomModeClassification.Xor:
                    return z - _centerZ;
                case CustomModeClassification.Default:
                    return z;
                case CustomModeClassification.Circle:
                    return Math.Pow(z - _centerZ, 2);
                default:
                    return z;
            }
        }

        private void Update()
        {
            if (!Input.GetKey(KeyCode.Space))
            {
                return;
            }
            ClassificationLibrary.trainModelLinearClassification(_model, _inputs.ToArray(), _dimensions, _spheres.Length, _expectedSigns, LearnStep, NumberOfIterations);

            foreach (var sphere in _spheresPlan)
            {
                var position = sphere.transform.position;
                double[] point;
                if (Mode == CustomModeClassification.Xor)
                {
                    point = new[] {MapPositionX(position.x) * MapPositionZ(position.z)};
                    
                }
                else if (Mode == CustomModeClassification.Cross)
                {
                    var posX = Math.Abs(position.x - _centerX);
                    var posZ = Math.Abs(position.z - _centerZ);
                    
                    var min = Math.Min(posX, posZ);
                    point = new[] {(double) Math.Abs(min) - _maxDistanceToAxis};
                }
                else
                {
                    point = new[] {MapPositionX(position.x), MapPositionZ(position.z)};
                }
                
                var newY = ClassificationLibrary.predictClassificationModel(_model, point, _dimensions);
                sphere.transform.position = new Vector3(position.x, newY, position.z);
            }
        }

        private void OnApplicationQuit()
        {
            ClassificationLibrary.releaseModel(_model);
        }
    }
}
