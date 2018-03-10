// dotnet new console
// dotnet add package Grpc --version 1.10.0
// dotnet add package Grpc.Core --version 1.10.0
// dotnet add package Grpc.Tools --version 1.10.0
// dotnet add package Google.Protobuf
// dotnet add package Microsoft.Windows.Compatibility --version 2.0.0-preview1-26216-03

using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Grpc.Core;
using Tensorflow.Serving;

namespace scoring_client
{
    class Program
    {
        static void Main(string[] args)
        {
            var imageFile = "test/input.bmp";
            var scoredImageFile = "test/output.bmp";
            var scoringServer = "10.0.1.85:9000";

            if (args.Length == 3) {
                imageFile = args[0];
                scoredImageFile = args[1];
                scoringServer = args[2];
            }

Stopwatch stopWatch = Stopwatch.StartNew();

            //Create gRPC Channel
			var channel = new Channel(scoringServer, ChannelCredentials.Insecure, 
                new List<Grpc.Core.ChannelOption> {
                    new ChannelOption(ChannelOptions.MaxReceiveMessageLength, int.MaxValue), 
                    new ChannelOption(ChannelOptions.MaxSendMessageLength, int.MaxValue) 
                });
			var client = new PredictionService.PredictionServiceClient(channel);

Console.WriteLine("Elapsed time {0} ms - gRPC Channel created",stopWatch.ElapsedMilliseconds);


            //Check available model
			var responce = client.GetModelMetadata(new GetModelMetadataRequest()
			{
				ModelSpec = new ModelSpec() { Name = "model" },
				MetadataField = { "signature_def" }
			});

			Console.WriteLine($"Model Available: {responce.ModelSpec.Name} Ver.{responce.ModelSpec.Version}");

Console.WriteLine("Elapsed time {0} ms - Model available",stopWatch.ElapsedMilliseconds);


            //Create prediction request
            var request = new PredictRequest()
            {
                ModelSpec = new ModelSpec() {Name = "model", SignatureName = "predict_image"}
            };

            //Add image tensor
            using (Stream stream = new FileStream(imageFile, FileMode.Open))
				{
					request.Inputs.Add("image", TensorBuilder.CreateTensorFromImage(stream, 1.0f));
				}
Console.WriteLine("Elapsed time {0} ms - image tensor created",stopWatch.ElapsedMilliseconds);


            // Run the prediction
            var predictResponse = client.Predict(request);


Console.WriteLine("Elapsed time {0} ms - prediction received",stopWatch.ElapsedMilliseconds);

            // Get predict output
            var scoredImage = predictResponse.Outputs["scored_image"];
            var image = TensorBuilder.CreateImageBitmapFromTensor(scoredImage, 1.0f);
            image.Save(scoredImageFile);

Console.WriteLine("Elapsed time {0} ms - image saved",stopWatch.ElapsedMilliseconds);
stopWatch.Stop();

            Console.WriteLine("output saved");
        }
    }
}