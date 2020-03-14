using System;
using System.IO;
using Microsoft.ML;
using System.Collections.Generic;
using System.Linq;
using AnomalyDetector.Utilities;

namespace AnomalyDetector.CLI
{
    class Program
    {
        static readonly string _dataPath = Path.Combine(Environment.CurrentDirectory, "Data");
        static void Main(string[] args)
        {
            // ProcessData<ProductSalesData>("product-sales.csv", nameof(ProductSalesData.numSales));
            // ProcessData<CoronavirusData>("full_data.csv", nameof(CoronavirusData.NewCases));

            var filePath = Path.Combine(_dataPath, "full_data.csv");

            AnomalyDetector<CoronavirusData, CoronavirusPrediction>
                .SetContext()
                .LoadDataFromFile(filePath)
                .ManipulateData(data =>
                        from dailyCounts in data
                        where dailyCounts.Location == "Worldwide"
                        let parsedDate = DateTime.Parse(dailyCounts.Date)
                        orderby parsedDate
                        select dailyCounts)
                .SetOptions(new AnomalyDetectorOptions(nameof(CoronavirusData.NewCases)))
                .DetectSpike();

        }

        // static void ProcessData<TDataType>(string fileName, string inputColumnName, char separatorCharacter = ',') where TDataType : class, new()
        // {
        //     var filePath = Path.Combine(_dataPath, fileName);

        //     AnomalyDetector<TDataType>
        //         .SetContext()
        //         .LoadDataFromFile(filePath)
        //         .SetOptions(new AnomalyOptions(inputColumnName, "Prediction"))
        //         .DetectSpike(Console.Out);
        // }
    }
}
