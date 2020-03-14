using System.Collections.Generic;
using Microsoft.ML;

public readonly struct AnomalyOptions
{
    public string InputColumnName {get;}

    public AnomalyOptions(string inputColumnName)
        => InputColumnName = inputColumnName;
}