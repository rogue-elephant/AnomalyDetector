using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.ML.Data;

public interface IAnomalyDetector<TInputType, TAnomalyOutputType> :
    IAnomalyDetectorLoadData<TInputType>,
    IAnomalyDetectorSetOptions<TInputType>,
    IAnomalyDetectorDetection<TInputType>
    where TInputType : class, new()
    where TAnomalyOutputType : class, IAnomalyDetectionOutput, new()
{
}

public interface IAnomalyDetectorLoadData<TInputType>
{
    IAnomalyDetectorSetOptions<TInputType> LoadDataFromFile(string path, bool hasHeaders = true, char seperator = ',');
    IAnomalyDetectorSetOptions<TInputType> LoadData(IEnumerable<TInputType> data);
}
public interface IAnomalyDetectorSetOptions<TInputType>
{
    IAnomalyDetectorDetection<TInputType> SetOptions(AnomalyOptions options);
    IAnomalyDetectorDetection<TInputType> ManipulateData(Action<IEnumerable<TInputType>> manipulations);
}

public interface IAnomalyDetectorDetection<TInputType> : IAnomalyDetectorSetOptions<TInputType>
{
    IAnomalyDetectorDetection<TInputType> DetectSpike(TextWriter output);
    IAnomalyDetectorDetection<TInputType> DetectChangepoint(TextWriter output);
}

public interface IAnomalyDetectionOutput
{
    double[] Prediction { get; set; }
}