using System;
using System.ClientModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.AI;

namespace InstaLike.Web.Media;

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

    private readonly IChatClient _chatClient;

    public ChatClientImageTagging(IChatClient chatClient)
    {
        _chatClient = chatClient ?? throw new ArgumentNullException(nameof(chatClient));
    }

    public async Task<Result<string[]>> AutoTagImage(Stream imageStream, string mediaType)
    {
        using var picture = new MemoryStream();
        await imageStream.CopyToAsync(picture);

        var request = new ChatMessage(ChatRole.User,
        [
            new TextContent(Instructions),
            new DataContent(picture.ToArray(), mediaType)
        ]);

        try
        {
            var response = await _chatClient.GetResponseAsync<TagSuggestions>(request);
            if (!response.TryGetResult(out var suggestions) || suggestions?.Tags is null)
            {
                return Result.Failure<string[]>("The picture could not be tagged.");
            }

            return Result.Success(
                suggestions.Tags
                    .Select(tag => $"#{tag.TrimStart('#')}".Replace(" ", string.Empty))
                    .Distinct()
                    .OrderBy(tag => tag)
                    .ToArray());
        }
        catch (ClientResultException ex)
        {
            return Result.Failure<string[]>(ex.Message);
        }
        catch (Exception ex) when (ex is HttpRequestException or AggregateException)
        {
            return Result.Failure<string[]>("The tagging service is not reachable.");
        }
    }
}

internal sealed record TagSuggestions(string[] Tags);
