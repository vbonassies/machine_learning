﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets
{
    public class MultiLayerClassification : MonoBehaviour
    {
        private GameObject[] _spheresPlan;
        private GameObject[] _spheres;
        private List<double> _inputs;
        private IntPtr _model;
        private IEnumerable<int> _expectedSigns;

        [SerializeField]
        private string _superParams = "2;1";

        public string SuperParams
        {
            get => _superParams;
            set => _superParams = value;
        }

        [SerializeField]
        private int _iterations = 1;

        public int Iterations
        {
            get => _iterations;
            set => _iterations = value;
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
            var superParam = SuperParams.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse)
                .ToArray();

            _spheresPlan = GameObject.FindGameObjectsWithTag("plan");
            _spheres = GameObject.FindGameObjectsWithTag("sphere");

            Debug.Log($"Sphere number : {_spheres.Length}");
            Debug.Log($"PlanSphere number : {_spheres.Length}");
            Debug.Log("Starting to call library for a LinearClassification");
            
            _model = ClassificationLibrary.createMultilayerModel(superParam, superParam.Length, LearnStep);

            _expectedSigns = _spheres.Select(sp => sp.transform.position.y < 0 ? -1 : 1);
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
            ClassificationLibrary.trainModelMultilayerClassification(_model, _inputs.ToArray(), _spheres.Length, _expectedSigns.ToArray(), Iterations);

            foreach (var sphere in _spheresPlan)
            {
                var position = sphere.transform.position;
                var point = new double[] { position.x, position.z };
                var newY = ClassificationLibrary.predictMultilayerClassificationModel(_model, point);
                sphere.transform.position = new Vector3(position.x, newY, position.z);
            }
        }

        private void OnApplicationQuit()
        {
            ClassificationLibrary.releaseMultilayerModel(_model);
        }
    }
}
