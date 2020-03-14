using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML;

public class AnomalyDetectorBase
{

}

public class AnomalyDetector<TInputType, TAnomalyOutputType> : IAnomalyDetector<TInputType, TAnomalyOutputType>
    where TInputType : class, new()
    where TAnomalyOutputType : class, IAnomalyDetectionOutput, new()
{
    private readonly MLContext _mlContext;
    private AnomalyOptions _options;
    private IDataView _dataView;
    private IEnumerable<TInputType> _data;
    protected AnomalyDetector(MLContext mlContext) => _mlContext = mlContext;

    public static IAnomalyDetectorLoadData<TInputType> SetContext(MLContext mlContext = null) => new AnomalyDetector<TInputType, TAnomalyOutputType>(mlContext ?? new MLContext());

    public IAnomalyDetectorSetOptions<TInputType> LoadData(IEnumerable<TInputType> data)
    {
        _data = data;
        return this;
    }
    public IAnomalyDetectorSetOptions<TInputType> LoadDataFromFile(string path, bool hasHeaders = true, char seperator = ',')
    {
        _dataView = _mlContext.Data.LoadFromTextFile<TInputType>(path: path, hasHeader: hasHeaders, separatorChar: seperator);
        return LoadData(_mlContext.Data.CreateEnumerable<TInputType>(_dataView, reuseRowObject: false));
    }

    public IAnomalyDetectorDetection<TInputType> SetOptions(AnomalyOptions options)
    {
        _options = options;
        return this;
    }

    public IAnomalyDetectorDetection<TInputType> ManipulateData(Action<IEnumerable<TInputType>> action)
    {
        action(_data);
        return this;
    }

    private IEnumerable<string> getColumnNames() => GetPropertiesAndFields(typeof(TAnomalyOutputType))
                .Where(prop => prop.name != nameof(IAnomalyDetectionOutput.Prediction))
                .Select(x => x.name);

    private IEnumerable<(string name, object? value)> GetPropertiesAndFields(Type type, object instance = null)
    {
        foreach (var typeItemTuple in
                (type.GetProperties()?.Select(x => (x.Name, instance != null ? x.GetValue(instance) : null)))
                .Concat(type.GetFields()?.Select(x => (x.Name, instance != null ? x.GetValue(instance) : null)))
        )
        yield return typeItemTuple;
    }
    private string getColumnValuesString(IEnumerable<string> columnNames, TAnomalyOutputType dataRow) =>
        string.Join('\t', GetPropertiesAndFields(typeof(TAnomalyOutputType), dataRow).Where(prop => columnNames.Contains(prop.name)).Select(x => x.value));


    /// <summary>
    /// The goal of spike detection is to identify sudden yet temporary bursts that significantly differ from the majority of the time series data values.
    /// It's important to detect these suspicious rare items, events, or observations in a timely manner to be minimized.
    /// The following approach can be used to detect a variety of anomalies such as: outages, cyber-attacks, or viral web content.
    /// </summary>
    public IAnomalyDetectorDetection<TInputType> DetectSpike(TextWriter output)
    {
        output.WriteLine("Detect temporary changes in pattern");

        // STEP 2: Set the training algorithm
        var iidSpikeEstimator = _mlContext.Transforms
        .DetectIidSpike(outputColumnName: nameof(IAnomalyDetectionOutput.Prediction), inputColumnName: _options.InputColumnName,
        confidence: 95, pvalueHistoryLength: _data.Count() / 4);

        // STEP 3: Create the transform
        // Create the spike detection transform
        output.WriteLine("=============== Training the model ===============");
        ITransformer iidSpikeTransform = iidSpikeEstimator.Fit(CreateEmptyDataView(_mlContext));

        output.WriteLine("=============== End of training process ===============");
        //Apply data transformation to create predictions.
        IDataView transformedData = iidSpikeTransform.Transform(_dataView);

        var predictions = _mlContext.Data.CreateEnumerable<TAnomalyOutputType>(transformedData, reuseRowObject: false);

        var columnNames = getColumnNames().ToList();

        output.WriteLine($"Alert\t{string.Join('\t', columnNames)}\tScore\tP-Value");

        foreach (var p in predictions)
        {
            var results = $"{p.Prediction?[0]}\t{getColumnValuesString(columnNames, p)}\t{p.Prediction?[1]:f2}\t{p.Prediction?[2]:F2}";

            if (p.Prediction[0] == 1)
            {
                results += " <-- Spike detected";
            }

            output.WriteLine(results);
        }
        output.WriteLine("");
        return this;
    }

    /// <summary>
    /// Change points are persistent changes in a time series event stream distribution of values, like level changes and trends.
    /// These persistent changes last much longer than spikes and could indicate catastrophic event(s).
    /// Change points are not usually visible to the naked eye, but can be detected in your data using approaches such as in the following method.
    /// </summary>
    public IAnomalyDetectorDetection<TInputType> DetectChangepoint(TextWriter output)
    {
        output.WriteLine("Detect Persistent changes in pattern");

        //STEP 2: Set the training algorithm 
        var iidChangePointEstimator = _mlContext.Transforms
            .DetectIidChangePoint(outputColumnName: nameof(IAnomalyDetectionOutput.Prediction), inputColumnName: _options.InputColumnName, confidence: 95,
            changeHistoryLength: _data.Count() / 4);

        //STEP 3: Create the transform
        output.WriteLine("=============== Training the model Using Change Point Detection Algorithm===============");
        var iidChangePointTransform = iidChangePointEstimator.Fit(CreateEmptyDataView(_mlContext));
        output.WriteLine("=============== End of training process ===============");

        //Apply data transformation to create predictions.
        IDataView transformedData = iidChangePointTransform.Transform(_dataView);
        var predictions = _mlContext.Data.CreateEnumerable<TAnomalyOutputType>(transformedData, reuseRowObject: false);

        output.WriteLine($"Alert\t{getColumnNames()}Score\tP-Value\tMartingale value");

        foreach (var p in predictions)
        {
            var results = $"{p.Prediction[0]}\t{p.Prediction[1]:f2}\t{p.Prediction[2]:F2}\t{p.Prediction[3]:F2}";

            if (p.Prediction[0] == 1)
            {
                results += " <-- alert is on, predicted changepoint";
            }
            output.WriteLine(results);
        }
        output.WriteLine("");
        return this;
    }

    static IDataView CreateEmptyDataView(MLContext mlContext)
    {
        // Create empty DataView. We just need the schema to call Fit() for the time series transforms
        IEnumerable<TInputType> enumerableData = new List<TInputType>();
        return mlContext.Data.LoadFromEnumerable(enumerableData);
    }
}

// public class AnomalyDetector<TAnomalyOutputType> : AnomalyDetector<TAnomalyOutputType, TAnomalyOutputType> where TAnomalyOutputType : class, IAnomalyDetectionOutput, new()
// {
//     protected AnomalyDetector(MLContext mlContext) : base(mlContext) { }
// }