using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace InstaLike.Web.Services
{
    public class AzureComputerVisionRecognition : IImageRecognitionService
    {
        private readonly string _apiKey;
        private readonly string _endpointUrl;
        private readonly ComputerVisionClient _client;

        private static readonly List<VisualFeatureTypes> Features = new List<VisualFeatureTypes>()
        {
            VisualFeatureTypes.Categories,
            VisualFeatureTypes.Description,
            VisualFeatureTypes.Faces,
            VisualFeatureTypes.Tags
        };

        public AzureComputerVisionRecognition(string apiKey, string endpointUrl)
        {
            _apiKey = apiKey;
            _endpointUrl = endpointUrl;

            _client = new ComputerVisionClient(
                new ApiKeyServiceClientCredentials(_apiKey),
                new DelegatingHandler[] { })
            {
                Endpoint = _endpointUrl
            };
        }
    
        public async Task<string[]> AutoTagImage(Stream imageStream)
        {
            ImageAnalysis result = await _client.AnalyzeImageInStreamAsync(imageStream, Features);
            return result.Tags.Select(tag => $"#{tag.Name}").ToArray();
      }
    }
}
