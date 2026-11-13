using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.SharedKernel;
using InstaLike.Web.Media;
using Serilog;
using Xunit;

namespace InstaLike.IntegrationTests
{
    public sealed class UnusedPictureCleanupFixture : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _testFixture;
        private readonly ITestOutputHelper _output;
        private readonly InMemoryPictureStore _pictureStore = new();

        public UnusedPictureCleanupFixture(DatabaseFixture fixture, ITestOutputHelper output)
        {
            _testFixture = fixture;
            _output = output;
        }

        [Fact]
        public async Task Should_Delete_Pictures_No_Entity_References()
        {
            var postPicture = await UploadAsync(PictureContainers.Posts);
            var unusedPostPicture = await UploadAsync(PictureContainers.Posts);
            var profilePicture = await UploadAsync(PictureContainers.Profiles);
            var unusedProfilePicture = await UploadAsync(PictureContainers.Profiles);
            var author = TestDataHelper.NewMember("cleanupuser1");
            author.SetProfilePicture(profilePicture);
            using (var session = _testFixture.OpenSession(_output))
            {
                await session.SaveAsync(author, TestContext.Current.CancellationToken);
                await session.SaveAsync(TestDataHelper.NewPost(author, "post with a used picture", postPicture), TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            await CleanUpUploadedBeforeAsync(DateTimeOffset.MaxValue);

            using (new AssertionScope())
            {
                _pictureStore.GetContent(PictureContainers.Posts, postPicture).Should().NotBeNull();
                _pictureStore.GetContent(PictureContainers.Profiles, profilePicture).Should().NotBeNull();
                _pictureStore.GetContent(PictureContainers.Posts, unusedPostPicture).Should().BeNull();
                _pictureStore.GetContent(PictureContainers.Profiles, unusedProfilePicture).Should().BeNull();
            }
        }

        [Fact]
        public async Task Should_Keep_Pictures_Uploaded_After_The_Cutoff()
        {
            var cutoff = DateTimeOffset.UtcNow;
            var recentPicture = await UploadAsync(PictureContainers.Posts);

            await CleanUpUploadedBeforeAsync(cutoff);

            _pictureStore.GetContent(PictureContainers.Posts, recentPicture).Should().NotBeNull("its transaction may still be running");
        }

        private Task<PictureId> UploadAsync(string container) =>
            _pictureStore.SaveAsync(container, Convert.FromBase64String(DatabaseFixture.GetTestPictureBase64()), TestContext.Current.CancellationToken);

        private async Task CleanUpUploadedBeforeAsync(DateTimeOffset uploadedBefore)
        {
            var sut = new UnusedPictureCleanupService(_testFixture.SessionFactory, _pictureStore, Log.Logger);
            await sut.CleanUpUploadedBeforeAsync(uploadedBefore, TestContext.Current.CancellationToken);
        }
    }
}
