using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML;

public sealed class AnomalyDetector<TData> : IAnomalyDetectorLoadData<TData>, IAnomalyDetectorDetection where TData : class, new()
{
    private readonly MLContext _mlContext;
    private AnomalyOptions _options;
    private IDataView _dataView;
    private IEnumerable<TData> _data;
    private AnomalyDetector(MLContext mlContext) => _mlContext = mlContext;

    public static IAnomalyDetectorLoadData<TData> SetContext(MLContext mlContext = null) => new AnomalyDetector<TData>(mlContext ?? new MLContext());

    public IAnomalyDetectorSetOptions LoadData(IEnumerable<TData> data)
    {
        _data = data;
        return this;
    }
    public IAnomalyDetectorSetOptions LoadDataFromFile(string path, bool hasHeaders = true, char seperator = ',')
    {
        _dataView = _mlContext.Data.LoadFromTextFile<TData>(path: path, hasHeader: hasHeaders, separatorChar: seperator);
        return LoadData(_mlContext.Data.CreateEnumerable<TData>(_dataView, reuseRowObject: false));
    }

    public IAnomalyDetectorDetection SetOptions(AnomalyOptions options)
    {
        _options = options;
        return this;
    }
    

    /// <summary>
    /// The goal of spike detection is to identify sudden yet temporary bursts that significantly differ from the majority of the time series data values.
    /// It's important to detect these suspicious rare items, events, or observations in a timely manner to be minimized.
    /// The following approach can be used to detect a variety of anomalies such as: outages, cyber-attacks, or viral web content.
    /// </summary>
    public IAnomalyDetectorDetection DetectSpike(TextWriter output)
    {
        output.WriteLine("Detect temporary changes in pattern");

        // STEP 2: Set the training algorithm
        var iidSpikeEstimator = _mlContext.Transforms
        .DetectIidSpike(outputColumnName: _options.OutputColumnName, inputColumnName: _options.InputColumnName,
        confidence: 95, pvalueHistoryLength: _data.Count() / 4);

        // STEP 3: Create the transform
        // Create the spike detection transform
        output.WriteLine("=============== Training the model ===============");
        ITransformer iidSpikeTransform = iidSpikeEstimator.Fit(CreateEmptyDataView(_mlContext));

        output.WriteLine("=============== End of training process ===============");
        //Apply data transformation to create predictions.
        IDataView transformedData = iidSpikeTransform.Transform(_dataView);

        var predictions = _mlContext.Data.CreateEnumerable<ProductSalesPrediction>(transformedData, reuseRowObject: false);

        output.WriteLine("Alert\tScore\tP-Value");

        foreach (var p in predictions)
        {
            var results = $"{p.Prediction[0]}\t{p.Prediction[1]:f2}\t{p.Prediction[2]:F2}";

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
    public IAnomalyDetectorDetection DetectChangepoint(TextWriter output)
    {
        output.WriteLine("Detect Persistent changes in pattern");

        //STEP 2: Set the training algorithm 
        var iidChangePointEstimator = _mlContext.Transforms
            .DetectIidChangePoint(outputColumnName: _options.OutputColumnName, inputColumnName: _options.InputColumnName, confidence: 95,
            changeHistoryLength: _data.Count() / 4);

        //STEP 3: Create the transform
        output.WriteLine("=============== Training the model Using Change Point Detection Algorithm===============");
        var iidChangePointTransform = iidChangePointEstimator.Fit(CreateEmptyDataView(_mlContext));
        output.WriteLine("=============== End of training process ===============");

        //Apply data transformation to create predictions.
        IDataView transformedData = iidChangePointTransform.Transform(_dataView);
        var predictions = _mlContext.Data.CreateEnumerable<ProductSalesPrediction>(transformedData, reuseRowObject: false);

        output.WriteLine("Alert\tScore\tP-Value\tMartingale value");

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
        IEnumerable<TData> enumerableData = new List<TData>();
        return mlContext.Data.LoadFromEnumerable(enumerableData);
    }
}