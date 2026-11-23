using System;
using System.ClientModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using InstaLike.Core.SharedKernel;
using Microsoft.Extensions.AI;

namespace InstaLike.Web.Media
{
    /// <summary>
    /// Suggests tags for a picture using a vision-capable chat model.
    /// </summary>
    internal sealed class ChatClientImageTagging : IImageRecognitionService
    {
        private const string Instructions =
            """
            You suggest tags for pictures shared on a social network.
            Describe the picture with 5 to 10 short, lowercase English tags.
            """;

        // Enough detail for tags: larger pictures only cost more tokens and upload time.
        private const int PictureMaxSizeInPixels = 768;

        private static readonly Error NoTagsSuggested =
            Error.Failure("Tagging.Failed", "The picture could not be tagged.");

        private static readonly Error ServiceUnreachable =
            Error.Failure("Tagging.Unavailable", "The tagging service is not reachable.");

        private static Error RejectedByService(string message) =>
            Error.Failure("Tagging.Failed", message);

        private readonly IChatClient _chatClient;

        public ChatClientImageTagging(IChatClient chatClient)
        {
            _chatClient = chatClient;
        }

        public async Task<Result<string[]>> AutoTagImage(Stream imageStream)
        {
            using var picture = new MemoryStream();
            await imageStream.CopyToAsync(picture);

            var jpeg = JpegPicture.ResizeTo(picture.ToArray(), PictureMaxSizeInPixels);
            if (jpeg.IsFailure)
            {
                return Result.Failure<string[]>(jpeg.Error);
            }

            var request = new ChatMessage(ChatRole.User,
            [
                new TextContent(Instructions),
                new DataContent(jpeg.Value, JpegPicture.MediaType)
            ]);

            try
            {
                var response = await _chatClient.GetResponseAsync<string[]>(request);
                if (!response.TryGetResult(out var tags))
                {
                    return Result.Failure<string[]>(NoTagsSuggested);
                }

                return Result.Ok(
                    tags
                        .Select(tag => $"#{tag.TrimStart('#')}".Replace(" ", string.Empty))
                        .Distinct()
                        .OrderBy(tag => tag)
                        .ToArray());
            }
            catch (ClientResultException ex)
            {
                return Result.Failure<string[]>(RejectedByService(ex.Message));
            }
            catch (Exception ex) when (ex is HttpRequestException or AggregateException)
            {
                return Result.Failure<string[]>(ServiceUnreachable);
            }
        }
    }
}
