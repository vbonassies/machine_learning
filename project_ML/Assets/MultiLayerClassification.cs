﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Assets
{
    public class MultiLayerClassification : MonoBehaviour
    {
        private GameObject[] _spheresPlan;
        private GameObject[] _spheres;
        private List<double> _inputs;
        private IntPtr _model;
        private int[] _expectedSigns;

        [SerializeField] private int[] _nplParams = {2, 1};

        public int[] NplParams
        {
            get => _nplParams;
            set => _nplParams = value;
        }

        [SerializeField]
        private int _iterations = 100;

        public int Iterations
        {
            get => _iterations;
            set => _iterations = value;
        }

        [SerializeField]
        private double _learnStep = 0.01;

        public double LearnStep
        {
            get => _learnStep;
            set => _learnStep = value;
        }

        private void Start()
        {
            _spheresPlan = GameObject.FindGameObjectsWithTag("plan");
            _spheres = GameObject.FindGameObjectsWithTag("sphere");

            Debug.Log($"Sphere number : {_spheres.Length}");
            Debug.Log($"PlanSphere number : {_spheresPlan.Length}");
            Debug.Log("Starting to call library for a LinearClassification");
            
            _model = ClassificationLibrary.createMultilayerModel(NplParams, NplParams.Length, LearnStep);

            _expectedSigns = _spheres.Select(sp => sp.transform.position.y < 0 ? -1 : 1).ToArray();
            _inputs = new List<double>();
            foreach (var sphere in _spheres)
            {
                _inputs.Add(sphere.transform.position.x);
                _inputs.Add(sphere.transform.position.z);
            }
        }

        private void Update()
        {
            if (!Input.GetKey(KeyCode.Space))
            {
                return;
            }
            ClassificationLibrary.trainModelMultilayerClassification(_model, _inputs.ToArray(), _spheres.Length, _expectedSigns, Iterations);

            foreach (var sphere in _spheresPlan)
            {
                var position = sphere.transform.position;
                var point = new double[] { position.x, position.z };
                var output = new double[NplParams[NplParams.Length - 1]];
                var result =  ClassificationLibrary.predictMultilayerClassificationModel(_model, point);
                Marshal.Copy(result, output, 0, NplParams[NplParams.Length - 1]);

                sphere.transform.position = new Vector3(position.x, (float)output[0], position.z);
            }
        }

        private void OnApplicationQuit()
        {
            ClassificationLibrary.releaseMultilayerModel(_model);
        }
    }
}
