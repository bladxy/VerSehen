using Microsoft.ML.Transforms.Image;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using VerSehen.MVVM.Model;
using Microsoft.ML.Vision;
using System.Diagnostics;

namespace VerSehen.Core
{
    public class ImageClassifier
    {
        public void TrainModel(string csvFileName)
        {
            var context = new MLContext();
            var data = context.Data.LoadFromTextFile<ImageData>(csvFileName, separatorChar: ',', hasHeader: true);
            var pipeline = context.Transforms.Conversion.MapValueToKey("Label")
              .Append(context.Transforms.LoadRawImageBytes("ImageFeature", "C:\\Users\\jaeger04\\Desktop\\Wallpapers\\SnakeBibliotek", "Image"))
              .Append(context.MulticlassClassification.Trainers.ImageClassification(new ImageClassificationTrainer.Options { LabelColumnName = "Label", FeatureColumnName = "ImageFeature" }))
              .Append(context.Transforms.Conversion.MapKeyToValue("PredictedLabel"));
            Debug.WriteLine(System.IO.Directory.GetCurrentDirectory());
            var model = pipeline.Fit(data);
            context.Model.Save(model, data.Schema, $"{Path.GetFileNameWithoutExtension(csvFileName)}.zip");
        }

        public string Predict(string imagePath)
        {
            var context = new MLContext();
            var model = context.Model.Load("model.zip", out var modelSchema);
            var predictor = context.Model.CreatePredictionEngine<ImageData, ModelOutput>(model);
            var imageData = new ImageData { Image = imagePath };
            var prediction = predictor.Predict(imageData);
            return prediction.Prediction;
        }

        public void CreateCsvFile(string folderPath, string labelProperty, string csvFileName)
        {
            using (var writer = new StreamWriter(csvFileName))
            {
                writer.WriteLine("Image,Label");

                foreach (var filename in Directory.EnumerateFiles(folderPath))
                {
                    if (Path.GetExtension(filename) == ".json")
                    {
                        var json = File.ReadAllText(filename);

                        var options = new JsonSerializerOptions();
                        options.Converters.Add(new PointConverter());
                        var state = JsonSerializer.Deserialize<State>(json, options);

                        var image = Path.ChangeExtension(filename, ".png");
                        var label = GetLabel(state, labelProperty);

                        writer.WriteLine($"{image},{label}");
                    }
                }
            }
        }

        private string GetLabel(State state, string labelProperty)
        {
            switch (labelProperty)
            {
                case "ApplePosition":
                    return state.ApplePosition.ToString();
                case "SnakeHeadPosition":
                    return state.SnakeHeadPosition.ToString();
                case "IsGameOver":
                    return state.IsGameOver ? "GameOver" : "NotGameOver";
                case "SnakeBodyPoints":
                    return string.Join(";", state.SnakeBodyPoints.Select(p => p.ToString()));
                default:
                    throw new ArgumentException($"Unknown label property: {labelProperty}");
            }
        }
    }
}

