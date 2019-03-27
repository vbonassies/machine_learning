﻿using System;
using System.Runtime.InteropServices;

namespace Assets
{
    public class ClassificationLibrary
    {
        #region Model Creation

        [DllImport("ClassificationDLL")]
        public static extern IntPtr createModel(int inputsDimension);

        [DllImport("ClassificationDLL")]
        public static extern IntPtr createMultilayerModel(int[] nplParams, int nbLayer, double learnStep);

        #endregion

        #region Prediction

        [DllImport("ClassificationDLL")]
        public static extern int predictClassificationModel(IntPtr model, double[] inputk, int inputsDimension);

        [DllImport("ClassificationDLL")]
        public static extern double predictRegressionModel(IntPtr model, double[] inputk, int inputsDimension);

        [DllImport("ClassificationDLL")]
        public static extern void predictMultilayerClassificationModel(IntPtr model, double[] inputk, double[] outputk);

        [DllImport("ClassificationDLL")]
        public static extern void predictMultilayerRegressionModel(IntPtr model, double[] inputk, double[] outputk);

        #endregion

        #region Training

        [DllImport("ClassificationDLL")]
        public static extern void trainModelLinearClassification(IntPtr model, double[] inputs, int inputsDimension, int nbInputs,
            int[] expectedSigns, double learnStep, int nbIterations);

        [DllImport("ClassificationDLL")]
        public static extern void trainModelLinearRegression(IntPtr model, double[] inputs, int inputsDimension,
            int nbInputs, double[] expectedSigns);

        [DllImport("ClassificationDLL")]
        public static extern void trainModelMultilayerClassification(IntPtr model, double[] inputs,
            int nbInputs, double[] expectedSigns, int iterations);

        [DllImport("ClassificationDLL")]
        public static extern void trainModelMultilayerRegression(IntPtr model, double[] inputs,
            int nbInputs, double[] expectedSigns, int iterations);

        #endregion

        #region Release

        [DllImport("ClassificationDLL")]
        public static extern void releaseModel(IntPtr model);

        [DllImport("ClassificationDLL")]
        public static extern void releaseMultilayerModel(IntPtr model);

        #endregion
    }
}

