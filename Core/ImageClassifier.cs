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

namespace VerSehen.Core
{
    public class ImageClassifier
    {
        public void TrainModel()
        {
            // Erstellen Sie einen neuen MLContext
            var context = new MLContext();

            // Laden Sie die Daten
            var data = context.Data.LoadFromTextFile<ImageData>("training_data.csv", separatorChar: ',');

            // Teilen Sie die Daten in Trainings- und Testdaten auf
            var trainTestSplit = context.Data.TrainTestSplit(data);

            // Definieren Sie die Pipeline
            var pipeline = context.Transforms.Conversion.MapValueToKey("Label")
               .Append(context.Transforms.LoadRawImageBytes("Image", "ImagePath"))
               .Append(context.MulticlassClassification.Trainers.ImageClassification())
               .Append(context.Transforms.Conversion.MapKeyToValue("PredictedLabel"));


            // Trainieren Sie das Modell
            var model = pipeline.Fit(trainTestSplit.TrainSet);

            // Evaluieren Sie das Modell
            var predictions = model.Transform(trainTestSplit.TestSet);
            var metrics = context.MulticlassClassification.Evaluate(predictions);

            // Speichern Sie das Modell
            context.Model.Save(model, trainTestSplit.TrainSet.Schema, "model.zip");
        }

        public string Predict(string imagePath)
        {
            var context = new MLContext();
            var model = context.Model.Load("model.zip", out var modelSchema);
            var predictor = context.Model.CreatePredictionEngine<ImageData, ModelOutput>(model);
            var imageData = new ImageData { ImagePath = imagePath };
            var prediction = predictor.Predict(imageData);
            return prediction.Prediction;
        }

        public void CreateCsvFile(string folderPath)
        {
            using (var writer = new StreamWriter("images.csv"))
            {
                writer.WriteLine("ImagePath,Label");

                foreach (var filename in Directory.EnumerateFiles(folderPath))
                {
                    if (Path.GetExtension(filename) == ".json")
                    {
                        var json = File.ReadAllText(filename);
                        var state = JsonSerializer.Deserialize<State>(json);

                        var imagePath = Path.ChangeExtension(filename, ".png");
                        var label = state.ApplePosition; // Sie müssen dies an Ihre spezifischen Labels anpassen

                        writer.WriteLine($"{imagePath},{label}");
                    }
                }
            }
        }

    }
}

