using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Image;
using Microsoft.ML.Vision;

// Definieren Sie eine Klasse für Ihre Eingabedaten
public class ImageData
{
    [LoadColumn(0)]
    public string Image;

    [LoadColumn(1)]
    public string Label;
}