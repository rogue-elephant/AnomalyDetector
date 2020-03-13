using System.Collections.Generic;
using System.IO;

public interface IAnomalyDetectorSetOptions
{
    IAnomalyDetectorDetection SetOptions(AnomalyOptions options);
}

public interface IAnomalyDetectorLoadData<TData>
{
    IAnomalyDetectorSetOptions LoadDataFromFile(string path, bool hasHeaders = true, char seperator = ',');
    IAnomalyDetectorSetOptions LoadData(IEnumerable<TData> data);
}

public interface IAnomalyDetectorDetection : IAnomalyDetectorSetOptions
{
    IAnomalyDetectorDetection DetectSpike(TextWriter output);
    IAnomalyDetectorDetection DetectChangepoint(TextWriter output);
}