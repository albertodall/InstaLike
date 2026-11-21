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
    public sealed class UnusedPictureCleanupFixture : IntegrationTestFixture
    {
        public UnusedPictureCleanupFixture(TestInfrastructureFactory infrastructure)
            : base(infrastructure)
        { }

        [Fact]
        public async Task Should_Delete_Pictures_No_Entity_References()
        {
            var postPicture = await UploadAsync(PictureContainers.Posts);
            var unusedPostPicture = await UploadAsync(PictureContainers.Posts);
            var profilePicture = await UploadAsync(PictureContainers.Profiles);
            var unusedProfilePicture = await UploadAsync(PictureContainers.Profiles);
            var author = TestDataHelper.NewMember("cleanupuser1");
            author.SetProfilePicture(profilePicture);
            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(author, TestContext.Current.CancellationToken);
                await session.SaveAsync(TestDataHelper.NewPost(author, "post with a used picture", postPicture), TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            await CleanUpUploadedBeforeAsync(DateTimeOffset.MaxValue);

            using (new AssertionScope())
            {
                (await Infrastructure.ReadPictureAsync(PictureContainers.Posts, postPicture)).Should().NotBeNull();
                (await Infrastructure.ReadPictureAsync(PictureContainers.Profiles, profilePicture)).Should().NotBeNull();
                (await Infrastructure.ReadPictureAsync(PictureContainers.Posts, unusedPostPicture)).Should().BeNull();
                (await Infrastructure.ReadPictureAsync(PictureContainers.Profiles, unusedProfilePicture)).Should().BeNull();
            }
        }

        [Fact]
        public async Task Should_Keep_Pictures_Uploaded_After_The_Cutoff()
        {
            var cutoff = DateTimeOffset.UtcNow.AddMinutes(-1);
            var recentPicture = await UploadAsync(PictureContainers.Posts);

            await CleanUpUploadedBeforeAsync(cutoff);

            (await Infrastructure.ReadPictureAsync(PictureContainers.Posts, recentPicture)).Should().NotBeNull("its transaction may still be running");
        }

        /// <summary>
        /// Uploads the test picture to the container, with no post or member referencing it.
        /// </summary>
        private Task<PictureId> UploadAsync(string container) =>
            Infrastructure.PictureStore.SaveAsync(container, Convert.FromBase64String(TestInfrastructureFactory.GetTestPictureBase64()), TestContext.Current.CancellationToken);

        /// <summary>
        /// Runs the cleanup on the pictures uploaded before the given time.
        /// </summary>
        private async Task CleanUpUploadedBeforeAsync(DateTimeOffset uploadedBefore)
        {
            var sut = new UnusedPictureCleanupService(Infrastructure.SessionFactory, Infrastructure.PictureStore, Log.Logger);
            await sut.CleanUpUploadedBeforeAsync(uploadedBefore, TestContext.Current.CancellationToken);
        }
    }
}
