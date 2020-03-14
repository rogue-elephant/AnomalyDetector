namespace AnomalyDetector.Utilities
{
    public readonly struct AnomalyDetectorOptions
    {
        public string InputColumnName { get; }

        public AnomalyDetectorOptions(string inputColumnName)
            => InputColumnName = inputColumnName;
    }
}