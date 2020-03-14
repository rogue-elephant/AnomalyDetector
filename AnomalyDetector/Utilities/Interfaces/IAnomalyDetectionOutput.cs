namespace AnomalyDetector.Utilities.Interfaces
{
    public interface IAnomalyDetectionOutput
    {
        double[] Prediction { get; set; }
    }
}