using Microsoft.ML;

public readonly struct AnomalyOptions
{
    public string InputColumnName {get;}
    public string OutputColumnName {get;}
    
    public AnomalyOptions(string inputColumnName, string outputColumnName)
        => (InputColumnName, OutputColumnName) = (inputColumnName, outputColumnName);
}