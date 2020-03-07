using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace InstaLike.Web.Services
{
    internal class AzureComputerVisionRecognition : IImageRecognitionService
    {
        private readonly string _apiKey;
        private readonly string _endpointUrl;

        private static readonly List<VisualFeatureTypes> Features = new List<VisualFeatureTypes>()
        {
            VisualFeatureTypes.Categories,
            VisualFeatureTypes.Description,
            VisualFeatureTypes.Faces,
            VisualFeatureTypes.Tags
        };

        public AzureComputerVisionRecognition(string apiKey, string endpointUrl)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _endpointUrl = endpointUrl ?? throw new ArgumentNullException(nameof(endpointUrl));
        }
    
        public async Task<Result<string[]>> AutoTagImage(Stream imageStream)
        {
            var apiCredentials = new ApiKeyServiceClientCredentials(_apiKey);
            using (var cvClient = new ComputerVisionClient(apiCredentials))
            {
                try
                {
                    cvClient.Endpoint = _endpointUrl;
                    var result = await cvClient.AnalyzeImageInStreamAsync(imageStream, Features);
                    return Result.Success(
                        result.Tags
                            .Select(tag => $"#{tag.Name}".Replace(" ", string.Empty))
                            .OrderBy(tag => tag)
                            .ToArray()
                        );
                }
                catch (ComputerVisionErrorException ex)
                {
                    return Result.Failure<string[]>(ex.Body != null ? ex.Body.Message : ex.Message);
                }
            }
        }
    }
}
