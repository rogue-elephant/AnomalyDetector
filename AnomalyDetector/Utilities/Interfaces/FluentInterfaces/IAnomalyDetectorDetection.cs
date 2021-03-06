using System;
using System.Collections.Generic;

namespace AnomalyDetector.Utilities.Interfaces.Fluent
{
    public interface IAnomalyDetectorSetOptions<TInputType>
    {
        IAnomalyDetectorDetection<TInputType> SetOptions(AnomalyDetectorOptions options);
        IAnomalyDetectorDetection<TInputType> ManipulateData(Func<IEnumerable<TInputType>, IEnumerable<TInputType>> manipulations);
    }
}