namespace AnomalyDetector.Utilities.Interfaces.Fluent
{
    public interface IAnomalyDetectorDetection<TInputType> : IAnomalyDetectorSetOptions<TInputType>
    {
        IAnomalyDetectorDetection<TInputType> DetectSpike();
        IAnomalyDetectorDetection<TInputType> DetectChangepoint();
    }
}