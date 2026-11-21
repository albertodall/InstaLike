using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Membership;
using InstaLike.Core.Posts;
using InstaLike.Core.SharedKernel;
using InstaLike.Web.Media;
using InstaLike.Web.Posts;
using NHibernate;
using NHibernate.Collection;
using Serilog;
using Xunit;

namespace InstaLike.IntegrationTests
{
    public sealed class PostFixture : IntegrationTestFixture
    {
        public PostFixture(TestInfrastructureFactory infrastructure)
            : base(infrastructure)
        { }

        [Fact]
        public async Task Should_Publish_A_New_Post()
        {
            const string testUserName = "testuser1";
            var testUser = TestDataHelper.NewMember(testUserName);
            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(testUser, TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var command = new PublishPostCommand(testUser.ID.Value, "test post 1", Convert.FromBase64String(TestInfrastructureFactory.GetTestPictureBase64()));
            var publishCommandResult = await SendAsync(command);

            using (new AssertionScope())
            {
                publishCommandResult.IsSuccess.Should().BeTrue();
                using (var session = Infrastructure.OpenSession())
                {
                    var publishedPost = await session.GetAsync<Post>(publishCommandResult.Value, TestContext.Current.CancellationToken);
                    publishedPost.Should().NotBeNull();
                    (await Infrastructure.ReadPictureAsync(PictureContainers.Posts, publishedPost.Picture)).Should().Equal(command.PictureRawBytes);
                }
            }
        }

        [Fact]
        public async Task Should_Not_Publish_A_Post_Without_A_Picture()
        {
            var author = TestDataHelper.NewMember("nopictureauthor");
            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(author, TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var command = new PublishPostCommand(author.ID.Value, "post without a picture", []);
            (await SendAsync(command)).IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Should_Put_a_Comment_On_A_Post()
        {
            var author = TestDataHelper.NewMember("author1");
            var commenter = TestDataHelper.NewMember("commenter1");
            Post post;
            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(author, TestContext.Current.CancellationToken);
                await session.SaveAsync(commenter, TestContext.Current.CancellationToken);
                post = TestDataHelper.NewPost(author);
                await session.SaveAsync(post, TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var command = new PublishCommentCommand(post.ID, "test comment", commenter.ID.Value);
            var commentPostResult = await SendAsync(command);

            using (new AssertionScope())
            {
                commentPostResult.IsSuccess.Should().BeTrue();
                using (var session = Infrastructure.OpenSession())
                {
                    (await session.GetAsync<Comment>(commentPostResult.Value, TestContext.Current.CancellationToken)).Should().NotBeNull();
                    (await session.QueryOver<Comment>().Where(c => c.Post.ID == post.ID).RowCountAsync(TestContext.Current.CancellationToken)).Should().Be(1);
                }
            }
        }

        [Fact]
        public async Task Commenting_A_Post_Should_Not_Load_Its_Existing_Comments()
        {
            Result<Guid> commentPostResult;
            var author = TestDataHelper.NewMember("author9");
            var reader = TestDataHelper.NewMember("reader9");
            Post post;
            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(author, TestContext.Current.CancellationToken);
                await session.SaveAsync(reader, TestContext.Current.CancellationToken);
                post = TestDataHelper.NewPost(author);
                await session.SaveAsync(post, TestContext.Current.CancellationToken);
                TestDataHelper.CommentOn(post, reader, "first comment");
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            using (var session = Infrastructure.OpenSession())
            {
                var sut = new TestMediatorBuilder(session, Infrastructure.PictureStore).Build();
                commentPostResult = await sut.Send(new PublishCommentCommand(post.ID, "second comment", reader.ID.Value), TestContext.Current.CancellationToken);

                using (new AssertionScope())
                {
                    commentPostResult.IsSuccess.Should().BeTrue();
                    CommentsOf(session, post.ID).WasInitialized.Should().BeFalse("adding a comment does not need the other comments");
                }
            }

            using (var session = Infrastructure.OpenSession())
            {
                (await session.QueryOver<Comment>().Where(c => c.Post.ID == post.ID).RowCountAsync(TestContext.Current.CancellationToken)).Should().Be(2);
            }
        }

        [Fact]
        public async Task Should_Put_A_Like_To_A_Post()
        {
            var author = TestDataHelper.NewMember("author2");
            var reader = TestDataHelper.NewMember("reader1");
            Post post;
            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(author, TestContext.Current.CancellationToken);
                await session.SaveAsync(reader, TestContext.Current.CancellationToken);
                post = TestDataHelper.NewPost(author);
                await session.SaveAsync(post, TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var command = new LikePostCommand(post.ID, reader.ID.Value);
            var likeResult = await SendAsync(command);

            using (new AssertionScope())
            {
                likeResult.IsSuccess.Should().BeTrue();
                using (var session = Infrastructure.OpenSession())
                {
                    (await session.QueryOver<Like>().Where(l => l.Post.ID == post.ID).RowCountAsync(TestContext.Current.CancellationToken)).Should().Be(1);
                }
            }
        }

        [Fact]
        public async Task A_Like_Put_Before_The_Post_Is_Saved_Should_Be_Stored_With_Its_Post()
        {
            var author = TestDataHelper.NewMember("author8");
            var reader = TestDataHelper.NewMember("reader8");
            Post post;
            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(author, TestContext.Current.CancellationToken);
                await session.SaveAsync(reader, TestContext.Current.CancellationToken);
                post = TestDataHelper.NewPost(author);
                post.LikeBy(reader.ID.Value);
                await session.SaveAsync(post, TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            using (var session = Infrastructure.OpenSession())
            {
                var storedPost = await session.GetAsync<Post>(post.ID, TestContext.Current.CancellationToken);
                storedPost.IsLikedBy(reader.ID.Value).Should().BeTrue();
            }
        }

        [Fact]
        public async Task Should_Dislike_A_Post()
        {
            var author = TestDataHelper.NewMember("author3");
            var reader = TestDataHelper.NewMember("reader2");
            Post post;
            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(author, TestContext.Current.CancellationToken);
                await session.SaveAsync(reader, TestContext.Current.CancellationToken);
                post = TestDataHelper.NewPost(author);
                await session.SaveAsync(post, TestContext.Current.CancellationToken);
                post.LikeBy(reader.ID.Value);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var command = new DislikePostCommand(post.ID, reader.ID.Value);
            var likeResult = await SendAsync(command);

            using (new AssertionScope())
            {
                likeResult.IsSuccess.Should().BeTrue();
                using (var session = Infrastructure.OpenSession())
                {
                    (await session.QueryOver<Like>().Where(l => l.Post.ID == post.ID).RowCountAsync(TestContext.Current.CancellationToken)).Should().Be(0);
                }
            }
        }

        [Fact]
        public async Task Cannot_Dislike_A_Post_Without_Putting_A_Like_First()
        {
            var author = TestDataHelper.NewMember("author4");
            var reader = TestDataHelper.NewMember("reader3");
            Post post;
            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(author, TestContext.Current.CancellationToken);
                await session.SaveAsync(reader, TestContext.Current.CancellationToken);
                post = TestDataHelper.NewPost(author);
                await session.SaveAsync(post, TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var command = new DislikePostCommand(post.ID, reader.ID.Value);
            var likeResult = await SendAsync(command);

            likeResult.IsSuccess.Should().BeFalse("the reader has not liked this post");
        }

        [Fact]
        public async Task Cannot_Like_A_Post_Twice()
        {
            Result likeResult;
            var author = TestDataHelper.NewMember("author5");
            var reader = TestDataHelper.NewMember("reader4");
            Post post;
            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(author, TestContext.Current.CancellationToken);
                await session.SaveAsync(reader, TestContext.Current.CancellationToken);
                post = TestDataHelper.NewPost(author);
                await session.SaveAsync(post, TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var command = new LikePostCommand(post.ID, reader.ID.Value);
            using (var session = Infrastructure.OpenSession())
            {
                var sut = new TestMediatorBuilder(session, Infrastructure.PictureStore).Build();
                await sut.Send(command, TestContext.Current.CancellationToken);
                likeResult = await sut.Send(command, TestContext.Current.CancellationToken);
            }

            likeResult.IsSuccess.Should().BeFalse("the reader has already liked this post");
        }

        [Fact]
        public async Task Author_Cannot_Put_Likes_On_Their_Own_Posts()
        {
            var author = TestDataHelper.NewMember("author6");
            Post post;
            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(author, TestContext.Current.CancellationToken);
                post = TestDataHelper.NewPost(author);
                await session.SaveAsync(post, TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var command = new LikePostCommand(post.ID, author.ID.Value);
            var likeResult = await SendAsync(command);

            likeResult.IsSuccess.Should().BeFalse("authors cannot like their own posts");
        }

        [Fact]
        public async Task Should_Find_One_Comment_For_Post()
        {
            CommentModel[] comments;
            var author = TestDataHelper.NewMember("author7");
            Post post;
            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(author, TestContext.Current.CancellationToken);
                post = TestDataHelper.NewPost(author);
                await session.SaveAsync(post, TestContext.Current.CancellationToken);
                post.CommentBy(author.ID.Value, CommentText.Create("First comment").Value);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var query = new PostCommentsQuery(post.ID);
            using (var session = Infrastructure.OpenStatelessSession())
            {
                var sut = new PostCommentsQueryHandler(session, Log.Logger);
                comments = await sut.Handle(query, TestContext.Current.CancellationToken);
            }

            comments.Length.Should().Be(1);
        }

        [Fact]
        public async Task Should_Read_A_Post_For_Editing()
        {
            Result<EditPostModel> result;
            var author = TestDataHelper.NewMember("editingauthor");
            Post post;
            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(author, TestContext.Current.CancellationToken);
                post = TestDataHelper.NewPost(author, "Text to edit");
                await session.SaveAsync(post, TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var query = new EditPostQuery(post.ID, author.ID.Value);
            using (var session = Infrastructure.OpenStatelessSession())
            {
                var sut = new EditPostQueryHandler(session, Log.Logger);
                result = await sut.Handle(query, TestContext.Current.CancellationToken);
            }

            using (new AssertionScope())
            {
                result.IsSuccess.Should().BeTrue();
                result.Value.PostID.Should().Be(post.ID);
                result.Value.AuthorID.Should().Be(author.ID.Value);
                result.Value.Text.Should().Be("Text to edit");
                result.Value.Picture.Should().Be(post.Picture);
            }
        }

        [Fact]
        public async Task Should_Not_Read_For_Editing_A_Post_Published_By_Another_User()
        {
            Result<EditPostModel> result;
            var author = TestDataHelper.NewMember("editingauthor2");
            var reader = TestDataHelper.NewMember("editingreader");
            Post post;
            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(author, TestContext.Current.CancellationToken);
                await session.SaveAsync(reader, TestContext.Current.CancellationToken);
                post = TestDataHelper.NewPost(author, "Not yours");
                await session.SaveAsync(post, TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var query = new EditPostQuery(post.ID, reader.ID.Value);
            using (var session = Infrastructure.OpenStatelessSession())
            {
                var sut = new EditPostQueryHandler(session, Log.Logger);
                result = await sut.Handle(query, TestContext.Current.CancellationToken);
            }

            result.Error.Should().Be(PostErrors.EditNotAllowed);
        }

        [Fact]
        public async Task Should_Not_Read_For_Editing_A_Missing_Post()
        {
            Result<EditPostModel> result;
            var missingPostID = PostId.New();

            var query = new EditPostQuery(missingPostID, Guid.NewGuid());
            using (var session = Infrastructure.OpenStatelessSession())
            {
                var sut = new EditPostQueryHandler(session, Log.Logger);
                result = await sut.Handle(query, TestContext.Current.CancellationToken);
            }

            result.Error.Should().Be(PostErrors.NotFound(missingPostID));
        }

        [Fact]
        public async Task Should_Load_All_Details_For_A_Post()
        {
            PostModel result;
            var author = TestDataHelper.NewMember("author7");
            var reader = TestDataHelper.NewMember("reader1");
            Post post;
            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(reader, TestContext.Current.CancellationToken);
                await session.SaveAsync(author, TestContext.Current.CancellationToken);
                post = TestDataHelper.NewPost(author);
                await session.SaveAsync(post, TestContext.Current.CancellationToken);
                post.CommentBy(reader.ID.Value, CommentText.Create("My comment").Value);
                await session.FlushAsync(TestContext.Current.CancellationToken);
                post.LikeBy(reader.ID.Value);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var query = new PostDetailQuery(post.ID, reader.ID.Value);
            using (var session = Infrastructure.OpenStatelessSession())
            {
                var sut = new PostDetailQueryHandler(session, Log.Logger);
                result = await sut.Handle(query, TestContext.Current.CancellationToken);
            }

            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.AuthorNickName.Should().Be(author.Nickname);
                result.LikesCount.Should().Be(1);
                result.Comments.Length.Should().Be(1);
                result.IsLikedByCurrentUser.Should().BeTrue();
                result.CanBeLikedByCurrentUser.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Should_Read_Posts_Created_By_Followed_Users()
        {
            TimelineModel timeline;
            var author1 = TestDataHelper.NewMember("authoruser1");
            var author2 = TestDataHelper.NewMember("authoruser2");
            var reader = TestDataHelper.NewMember("readeruser");
            Post post1;
            Post post2;
            Post post3;
            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(author1, TestContext.Current.CancellationToken);
                await session.SaveAsync(author2, TestContext.Current.CancellationToken);
                await session.SaveAsync(reader, TestContext.Current.CancellationToken);
                post1 = TestDataHelper.NewPost(author1, "test post 1");
                await session.SaveAsync(post1, TestContext.Current.CancellationToken);
                post2 = TestDataHelper.NewPost(author1, "test post 2");
                await session.SaveAsync(post2, TestContext.Current.CancellationToken);
                post3 = TestDataHelper.NewPost(author2, "test post 3");
                await session.SaveAsync(post3, TestContext.Current.CancellationToken);
                await session.SaveAsync(TestDataHelper.Following(reader, author1), TestContext.Current.CancellationToken);
                TestDataHelper.CommentOn(post1, reader, "nice picture");
                post1.LikeBy(reader.ID.Value);
                await session.FlushAsync(TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var query = new TimelineQuery(reader.ID.Value, 5);
            using (var session = Infrastructure.OpenStatelessSession())
            {
                var sut = new TimelineQueryHandler(session, Log.Logger);
                timeline = await sut.Handle(query, TestContext.Current.CancellationToken);
            }

            using (new AssertionScope())
            {
                timeline.Posts.Length.Should().Be(2);
                timeline.HasOlderPosts.Should().BeFalse();
                timeline.Posts.Should().OnlyContain(p => p.AuthorNickName == author1.Nickname);
                var commentedPost = timeline.Posts.Single(p => p.PostID == post1.ID);
                commentedPost.Comments.Should().ContainSingle();
                commentedPost.Comments[0].AuthorNickName.Should().Be(reader.Nickname);
                commentedPost.Comments[0].Text.Should().Be("nice picture");
                commentedPost.LikesCount.Should().Be(1);
                commentedPost.IsLikedByCurrentUser.Should().BeTrue();
                var otherPost = timeline.Posts.Single(p => p.PostID == post2.ID);
                otherPost.Comments.Should().BeEmpty();
                otherPost.LikesCount.Should().Be(0);
                otherPost.IsLikedByCurrentUser.Should().BeFalse();
            }
        }

        [Fact]
        public async Task Timeline_Reads_Older_Posts_In_Batches_Without_Repeating_Any()
        {
            var author = TestDataHelper.NewMember("batchauthor");
            var reader = TestDataHelper.NewMember("batchreader");
            var posts = Enumerable.Range(1, 5).Select(i => TestDataHelper.NewPost(author, $"batch post {i}")).ToArray();
            await SaveTimeline(author, reader, posts);

            var batches = await ReadTimelineInBatchesOfTwo(reader);

            using (new AssertionScope())
            {
                batches.Select(b => b.Posts.Length).Should().Equal(2, 2, 1);
                batches.Select(b => b.HasOlderPosts).Should().Equal(true, true, false);
                var timeline = batches.SelectMany(b => b.Posts).ToArray();
                timeline.Select(p => p.PostID).Should().BeEquivalentTo(posts.Select(p => p.ID));
                timeline.Select(p => p.PostDate).Should().BeInDescendingOrder();
            }
        }

        [Fact]
        public async Task Timeline_Reads_Posts_Published_At_The_Same_Time_In_Batches_Without_Repeating_Any()
        {
            var author = TestDataHelper.NewMember("sametimeauthor");
            var reader = TestDataHelper.NewMember("sametimereader");
            var posts = Enumerable.Range(1, 4).Select(i => TestDataHelper.NewPost(author, $"same time post {i}")).ToArray();
            await SaveTimeline(author, reader, posts);
            using (var session = Infrastructure.OpenSession())
            {
                await session.CreateQuery("update Post set PostDate = :date where AuthorID = :author")
                    .SetParameter("date", new DateTimeOffset(2026, 11, 15, 10, 0, 0, TimeSpan.Zero))
                    .SetParameter("author", author.ID.Value)
                    .ExecuteUpdateAsync(TestContext.Current.CancellationToken);
            }

            var batches = await ReadTimelineInBatchesOfTwo(reader);

            using (new AssertionScope())
            {
                batches.Select(b => b.Posts.Length).Should().Equal(2, 2);
                batches.Select(b => b.HasOlderPosts).Should().Equal(true, false);
                batches.SelectMany(b => b.Posts).Select(p => p.PostID).Should().BeEquivalentTo(posts.Select(p => p.ID));
            }
        }

        [Fact]
        public async Task User_Can_Edit_His_Own_Posts()
        {
            var picture = await Infrastructure.PictureStore.SaveAsync(PictureContainers.Posts, Convert.FromBase64String(TestInfrastructureFactory.GetTestPictureBase64()), TestContext.Current.CancellationToken);
            var author = TestDataHelper.NewMember("authoruser1");
            Post post;
            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(author, TestContext.Current.CancellationToken);
                post = TestDataHelper.NewPost(author, "test post 1", picture);
                await session.SaveAsync(post, TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var command = new EditPostCommand(author.ID.Value, post.ID, "edited text on post 1", Convert.FromBase64String(TestInfrastructureFactory.GetTestPictureBase64()));
            var editResult = await SendAsync(command);

            using (var session = Infrastructure.OpenSession())
            {
                using (new AssertionScope())
                {
                    var editedPost = session.Load<Post>(post.ID);
                    editedPost.Text.Value.Should().Be("edited text on post 1");
                    editedPost.Picture.Should().NotBe(post.Picture, "stored pictures are never overwritten");
                    editResult.IsSuccess.Should().BeTrue();
                }
            }
        }

        [Fact]
        public async Task User_Cannot_Edit_Post_Published_By_Another_User()
        {
            var picture = PictureId.New();
            var author = TestDataHelper.NewMember("authoruser1");
            var reader = TestDataHelper.NewMember("readeruser");
            Post post;
            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(author, TestContext.Current.CancellationToken);
                await session.SaveAsync(reader, TestContext.Current.CancellationToken);
                post = TestDataHelper.NewPost(author, "test post 1", picture);
                await session.SaveAsync(post, TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var command = new EditPostCommand(reader.ID.Value, post.ID, "edited text on post 1", Convert.FromBase64String(TestInfrastructureFactory.GetTestPictureBase64()));
            var editResult = await SendAsync(command);

            editResult.Error.Should().Be(PostErrors.EditNotAllowed);
        }

        [Fact]
        public async Task Should_Not_Publish_A_Post_With_An_Invalid_Text()
        {
            var author = TestDataHelper.NewMember("invalidtextauthor");
            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(author, TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var command = new PublishPostCommand(author.ID.Value, new string('x', 501), Convert.FromBase64String(TestInfrastructureFactory.GetTestPictureBase64()));
            var publishResult = await SendAsync(command);

            using (new AssertionScope())
            {
                publishResult.IsFailure.Should().BeTrue();
                publishResult.Error.Should().Be(PostErrors.TextTooLong);
                (await Infrastructure.PictureStore.ListUploadedBeforeAsync(PictureContainers.Posts, DateTimeOffset.MaxValue, TestContext.Current.CancellationToken).CountAsync(TestContext.Current.CancellationToken)).Should().Be(0);
            }
        }

        [Fact]
        public async Task Should_Not_Edit_A_Post_With_An_Invalid_Text()
        {
            var author = TestDataHelper.NewMember("invalideditauthor");
            var post = TestDataHelper.NewPost(author, "original text");
            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(author, TestContext.Current.CancellationToken);
                await session.SaveAsync(post, TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var command = new EditPostCommand(author.ID.Value, post.ID, new string('x', 501), Convert.FromBase64String(TestInfrastructureFactory.GetTestPictureBase64()));
            var editResult = await SendAsync(command);

            using (new AssertionScope())
            {
                editResult.IsFailure.Should().BeTrue();
                editResult.Error.Should().Be(PostErrors.TextTooLong);
                (await Infrastructure.PictureStore.ListUploadedBeforeAsync(PictureContainers.Posts, DateTimeOffset.MaxValue, TestContext.Current.CancellationToken).CountAsync(TestContext.Current.CancellationToken)).Should().Be(0);
            }

            using (var session = Infrastructure.OpenSession())
            {
                (await session.GetAsync<Post>(post.ID, TestContext.Current.CancellationToken)).Text.Value.Should().Be("original text");
            }
        }

        [Fact]
        public async Task Cannot_Edit_A_Post_That_Does_Not_Exist()
        {
            var missingPostID = PostId.New();
            var author = TestDataHelper.NewMember("authoruser2");
            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(author, TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var command = new EditPostCommand(author.ID.Value, missingPostID, "edited text", Convert.FromBase64String(TestInfrastructureFactory.GetTestPictureBase64()));
            var editResult = await SendAsync(command);

            editResult.IsFailure.Should().BeTrue();
            editResult.Error.Should().Be(PostErrors.NotFound(missingPostID));
        }

        [Fact]
        public async Task A_New_Post_Should_Be_Stored_When_The_Session_Is_Flushed()
        {
            var author = TestDataHelper.NewMember("flushauthor");
            using var session = Infrastructure.OpenSession();
            await session.SaveAsync(author, TestContext.Current.CancellationToken);
            await session.FlushAsync(TestContext.Current.CancellationToken);
            var post = TestDataHelper.NewPost(author);

            await session.SaveAsync(post, TestContext.Current.CancellationToken);
            var storedBeforeFlush = await CountPostsAsync(post.ID);
            await session.FlushAsync(TestContext.Current.CancellationToken);

            using (new AssertionScope())
            {
                storedBeforeFlush.Should().Be(0);
                (await CountPostsAsync(post.ID)).Should().Be(1);
            }
        }

        [Fact]
        public async Task Liking_And_Commenting_A_Post_Should_Only_Insert_The_New_Rows()
        {
            var author = TestDataHelper.NewMember("insertauthor");
            var reader = TestDataHelper.NewMember("insertreader");
            var post = TestDataHelper.NewPost(author);
            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(author, TestContext.Current.CancellationToken);
                await session.SaveAsync(reader, TestContext.Current.CancellationToken);
                await session.SaveAsync(post, TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var statements = new XUnitSqlStatementOutputInterceptor();

            using (var session = Infrastructure.SessionFactory.WithOptions().Interceptor(statements).OpenSession())
            {
                var storedPost = await session.GetAsync<Post>(post.ID, TestContext.Current.CancellationToken);
                storedPost.LikeBy(reader.ID.Value);
                storedPost.CommentBy(reader.ID.Value, (CommentText)"test comment");
                var statementsBeforeFlush = statements.Statements.Count;

                await session.FlushAsync(TestContext.Current.CancellationToken);

                statements.Statements.Skip(statementsBeforeFlush).Should().HaveCount(2)
                    .And.OnlyContain(s => s.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        /// How many posts are saved with the ID: 1 or 0.
        /// </summary>
        private async Task<int> CountPostsAsync(PostId postID)
        {
            using var session = Infrastructure.OpenStatelessSession();
            return await session.QueryOver<Post>()
                .Where(p => p.ID == postID)
                .RowCountAsync(TestContext.Current.CancellationToken);
        }

        /// <summary>
        /// The post's comments collection as the session tracks it, to check whether it was loaded.
        /// </summary>
        private static IPersistentCollection CommentsOf(ISession session, PostId postID)
        {
            var role = $"{typeof(Post).FullName}.{nameof(Post.Comments)}";
            return session.GetSessionImplementation().PersistenceContext.CollectionEntries.Keys
                .OfType<IPersistentCollection>()
                .Single(c => c.Role == role && Equals(c.Key, postID));
        }

        /// <summary>
        /// Saves the author's posts and makes the reader follow the author, so the posts are on the reader's timeline.
        /// </summary>
        private async Task SaveTimeline(Member author, Member reader, Post[] posts)
        {
            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(author, TestContext.Current.CancellationToken);
                await session.SaveAsync(reader, TestContext.Current.CancellationToken);
                foreach (var post in posts)
                {
                    await session.SaveAsync(post, TestContext.Current.CancellationToken);
                }
                await session.SaveAsync(TestDataHelper.Following(reader, author), TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }
        }

        /// <summary>
        /// Reads the reader's whole timeline as the page scrolls it: a first batch, then older posts until there are none.
        /// </summary>
        private async Task<TimelineModel[]> ReadTimelineInBatchesOfTwo(Member reader)
        {
            const int BatchSize = 2;
            var batches = new List<TimelineModel>
            {
                await ReadWithTimelineHandler(sut => sut.Handle(new TimelineQuery(reader.ID.Value, BatchSize), TestContext.Current.CancellationToken))
            };

            while (batches[^1].HasOlderPosts)
            {
                var lastPostShownID = batches[^1].Posts[^1].PostID;
                batches.Add(await ReadWithTimelineHandler(sut => sut.Handle(new OlderTimelinePostsQuery(reader.ID.Value, lastPostShownID, BatchSize), TestContext.Current.CancellationToken)));
            }

            return batches.ToArray();
        }

        /// <summary>
        /// Runs a timeline query on a new stateless session, as the application does.
        /// </summary>
        private async Task<TimelineModel> ReadWithTimelineHandler(Func<TimelineQueryHandler, Task<TimelineModel>> read)
        {
            using (var session = Infrastructure.OpenStatelessSession())
            {
                return await read(new TimelineQueryHandler(session, Log.Logger));
            }
        }
    }
}
