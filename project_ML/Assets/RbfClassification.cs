﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets
{
    public class RbfClassification : MonoBehaviour
    {
        [SerializeField]
        private double _gamma = 0.01;

        public double Gamma
        {
            get => _gamma;
            set => _gamma = value;
        }

        private void Start()
        {
            var spheresPlan = GameObject.FindGameObjectsWithTag("plan");
            var spheres = GameObject.FindGameObjectsWithTag("sphere");

            Debug.Log($"Sphere number : {spheres.Length}");
            Debug.Log($"PlanSphere number : {spheresPlan.Length}");
            Debug.Log("Starting to call library for a RBFClassification");

            var expectedSigns = spheres.Select(sp => sp.transform.position.y < 0 ? -1 : 1).ToArray();
            var inputs = new List<double>();
            foreach (var sphere in spheres)
            {
                var position = sphere.transform.position;
                inputs.Add(position.x);
                inputs.Add(position.z);
            }


            var model = ClassificationLibrary.createRbfModel(spheres.Length, inputs.ToArray(), Gamma);
            ClassificationLibrary.trainRbfModelClassification(model, expectedSigns);

            foreach (var sphere in spheresPlan)
            {
                var position = sphere.transform.position;
                var point = new double[] { position.x, position.z };

                var newY = ClassificationLibrary.predictRbfModelClassification(model, point);
                if (double.IsNaN(newY))
                {
                    ClassificationLibrary.releaseRbfModel(model);
                    Debug.LogError("It seems that some of exemples inputs are in collision");
                    return;
                }
                sphere.transform.position = new Vector3(position.x, newY, position.z);
            }

            ClassificationLibrary.releaseRbfModel(model);
        }
    }
}
