﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets
{
   
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

        [SerializeField]
        private int _numberOfIterations = 1;

        public int NumberOfIterations
        {
            get => _numberOfIterations;
            set => _numberOfIterations = value;
        }

        [SerializeField]
        private CustomMode _mode = CustomMode.Default;

        public CustomMode Mode
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
            _dimensions = _mode == CustomMode.Xor ? 1 : 2;
            _spheresPlan = GameObject.FindGameObjectsWithTag("plan");
            _spheres = GameObject.FindGameObjectsWithTag("sphere");

            Debug.Log($"Sphere number : {_spheres.Length}");
            Debug.Log($"PlanSphere number : {_spheresPlan.Length}");
            Debug.Log("Starting to call library for a LinearClassification");

            _model = ClassificationLibrary.createModel(_dimensions);

            if (Mode == CustomMode.Circle)
            {
                var allPointsWith1 = _spheres.Where(sp => sp.transform.position.y > 0).ToList();
                float totalX = 0, totalZ = 0;
                foreach (var p in allPointsWith1)
                {
                    var position = p.transform.position;
                    totalX += position.x;
                    totalZ += position.z;
                }
                _centerX = totalX / allPointsWith1.Count();
                _centerZ = totalZ / allPointsWith1.Count();
            }

            if (Mode == CustomMode.Xor)
            {
                var allPointsWith1 = _spheresPlan;
                float totalX = 0, totalZ = 0;
                foreach (var p in allPointsWith1)
                {
                    var position = p.transform.position;
                    totalX += position.x;
                    totalZ += position.z;
                }
                _centerX = totalX / allPointsWith1.Count();
                _centerZ = totalZ / allPointsWith1.Count();
            }
            

            Debug.Log("Found center at X = "+_centerX+" | Z = "+_centerZ);
            
            _expectedSigns = _spheres.Select(sp => sp.transform.position.y < 0 ? -1 : 1).ToArray();
            _inputs = new List<double>();
            foreach (var sphere in _spheres)
            {
                var position = sphere.transform.position;
                if (Mode != CustomMode.Xor)
                {
                    _inputs.Add(MapPositionX(position.x));
                    _inputs.Add(MapPositionZ(position.z));   
                }
                else
                {
                    _inputs.Add(MapPositionX(position.x) * MapPositionZ(position.z));
                }
            }
        }

        private double MapPositionX(double x)
        {
            switch (Mode)
            {
                case CustomMode.Xor:
                    return x - _centerX;
                case CustomMode.Default:
                    return x;
                case CustomMode.Circle:
                    return Math.Pow(x - _centerX, 2);
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }
        
        private double MapPositionZ(double z)
        {
            switch (Mode)
            {
                case CustomMode.Xor:
                    return z - _centerZ;
                case CustomMode.Default:
                    return z;
                case CustomMode.Circle:
                    return Math.Pow(z - _centerZ, 2);
                default:
                    throw new ArgumentOutOfRangeException();
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
                var point = Mode != CustomMode.Xor ? new[] {MapPositionX(position.x), MapPositionZ(position.z)} : new[] {MapPositionX(position.x) * MapPositionZ(position.z)};

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
