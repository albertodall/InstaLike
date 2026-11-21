using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using InstaLike.Web.Media;
using Microsoft.Extensions.AI;
using Xunit;

namespace InstaLike.Web.Tests
{
    public sealed class ImageTaggingFixture
    {
        [Fact]
        public async Task Suggested_Tags_Should_Be_Formatted_As_Sorted_Unique_Hashtags()
        {
            var chatClient = new FakeChatClient(
                """
                { "tags": ["tag1", "tag2", "#tag3", "tag4"] }
                """);
            var tagging = new ChatClientImageTagging(chatClient);

            var result = await tagging.AutoTagImage(new MemoryStream([1, 2, 3]), "image/jpeg");

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Equal("#tag1", "#tag2", "#tag3", "#tag4");
        }

        [Fact]
        public async Task Picture_Should_Be_Sent_To_The_Model_With_Its_Media_Type()
        {
            var chatClient = new FakeChatClient("""{ "tags": ["cat"] }""");
            var tagging = new ChatClientImageTagging(chatClient);

            await tagging.AutoTagImage(new MemoryStream(new byte[] { 1, 2, 3 }), "image/png");

            var picture = chatClient.ReceivedMessages.SelectMany(m => m.Contents).OfType<DataContent>().Single();
            picture.MediaType.Should().Be("image/png");
            picture.Data.ToArray().Should().Equal(1, 2, 3);
        }

        [Fact]
        public async Task Unexpected_Model_Response_Should_Return_A_Failure()
        {
            var tagging = new ChatClientImageTagging(new FakeChatClient("I cannot see any picture."));

            var result = await tagging.AutoTagImage(new MemoryStream(new byte[] { 1, 2, 3 }), "image/jpeg");

            result.IsFailure.Should().BeTrue();
        }

        [Fact]
        public async Task Unreachable_Model_Should_Return_A_Failure()
        {
            var unreachable = new AggregateException(new HttpRequestException("Connection refused"));
            var tagging = new ChatClientImageTagging(new FakeChatClient(unreachable));

            var result = await tagging.AutoTagImage(new MemoryStream(new byte[] { 1, 2, 3 }), "image/jpeg");

            result.IsFailure.Should().BeTrue();
        }

        /// <summary>
        /// Answers with a fixed text and keeps the messages it receives.
        /// </summary>
        private sealed class FakeChatClient : IChatClient
        {
            private readonly string _responseText = string.Empty;
            private readonly Exception? _failure;

            public FakeChatClient(string responseText)
            {
                _responseText = responseText;
            }

            public FakeChatClient(Exception failure)
            {
                _failure = failure;
            }

            public List<ChatMessage> ReceivedMessages { get; } = new();

            public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
            {
                ReceivedMessages.AddRange(messages);
                return _failure is null
                    ? Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, _responseText)))
                    : Task.FromException<ChatResponse>(_failure);
            }

            public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
                => throw new NotSupportedException();

            public object? GetService(Type serviceType, object? serviceKey = null) => null;

            public void Dispose() { }
        }
    }
}
