using System;
using System.IO;
using Microsoft.ML;
using System.Collections.Generic;
using System.Linq;

namespace Covid19Analyser
{
    class Program
    {
        static readonly string _dataPath = Path.Combine(Environment.CurrentDirectory, "Data");
        static void Main(string[] args)
        {
            ProcessData<ProductSalesData>("product-sales.csv", nameof(ProductSalesData.numSales));
            ProcessData<CoronavirusData>("full_data.csv", nameof(CoronavirusData.NewCases));
        }

        static void ProcessData<TDataType>(string fileName, string inputColumnName, char separatorCharacter = ',') where TDataType : class, new()
        {
            var filePath = Path.Combine(_dataPath, fileName);
            // Create MLContext to be shared across the model creation workflow objects 
            MLContext mlContext = new MLContext();

            // //STEP 1: Common data loading configuration
            // IDataView dataView = mlContext.Data.LoadFromTextFile<TDataType>(path: filePath, hasHeader: true, separatorChar: separatorCharacter);
            // var values = mlContext.Data.CreateEnumerable<TDataType>(dataView, reuseRowObject: false);
            // var count = values.Count();

            AnomalyDetector<TDataType>
                .SetContext()
                .LoadDataFromFile(filePath)
                .SetOptions(new AnomalyOptions(inputColumnName, "Prediction"))
                .DetectSpike(Console.Out);
        }
    }
}
