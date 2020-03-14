using System;
using AnomalyDetector.Utilities.Interfaces;
using Microsoft.ML.Data;

namespace AnomalyDetector.CLI
{
    public class CoronavirusData
    {
        [LoadColumn(0)]
        public string Date;
        [LoadColumn(1)]
        public string Location;
        [LoadColumn(2)]
        public Single NewCases;
        [LoadColumn(3)]
        public Single NewDeaths;
        [LoadColumn(4)]
        public Single TotalCases;
        [LoadColumn(5)]
        public Single TotalDeaths;
    }

    public class CoronavirusPrediction : CoronavirusData, IAnomalyDetectionOutput
    {
        [VectorType(3)]
        public double[] Prediction { get; set; }
    }
}