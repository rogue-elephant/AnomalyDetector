namespace AnomalyDetector.Utilities.Interfaces.Fluent
{
    public interface IAnomalyDetector<TInputType, TAnomalyOutputType> :
    IAnomalyDetectorLoadData<TInputType>,
    IAnomalyDetectorSetOptions<TInputType>,
    IAnomalyDetectorDetection<TInputType>
    where TInputType : class, new()
    where TAnomalyOutputType : class, IAnomalyDetectionOutput, new()
    {
    }
}