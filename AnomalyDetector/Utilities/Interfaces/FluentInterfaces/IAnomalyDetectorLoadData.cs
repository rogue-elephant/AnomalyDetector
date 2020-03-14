using System;
using System.Collections.Generic;

namespace AnomalyDetector.Utilities.Interfaces.Fluent
{
    public interface IAnomalyDetectorLoadData<TInputType>
    {
        IAnomalyDetectorSetOptions<TInputType> LoadDataFromFile(string path, bool hasHeaders = true, char seperator = ',');
        IAnomalyDetectorSetOptions<TInputType> LoadData(IEnumerable<TInputType> data);
    }
}