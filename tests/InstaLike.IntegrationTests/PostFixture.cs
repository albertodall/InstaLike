using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Accounts;
using InstaLike.Core.Posts;
using InstaLike.Core.SharedKernel;
using InstaLike.Core.Social;
using InstaLike.Web.Media;
using InstaLike.Web.Models;
using InstaLike.Web.Posts.Commands;
using InstaLike.Web.Posts.Queries;
using NHibernate;
using NHibernate.Collection;
using Serilog;
using Xunit;

namespace InstaLike.IntegrationTests;

public sealed class PostFixture : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _testFixture;
    private readonly ITestOutputHelper _output;
    private readonly InMemoryPictureStore _pictureStore = new();

    public PostFixture(DatabaseFixture fixture, ITestOutputHelper output)
    {
        _testFixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task Should_Publish_A_New_Post()
    {
        const string testUserName = "testuser1";
        Result<PostId> publishCommandResult;

        var testUser = TestDataHelper.NewUser(testUserName);
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(testUser, TestContext.Current.CancellationToken);
            await session.FlushAsync(TestContext.Current.CancellationToken);
        }

        var command = new PublishPostCommand(testUser.ID.Value, "test post 1", Convert.FromBase64String(DatabaseFixture.GetTestPictureBase64()));
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = new TestMediatorBuilder(session).WithPictureStore(_pictureStore).Build();
            publishCommandResult = await sut.Send(command, TestContext.Current.CancellationToken);
        }

        using (new AssertionScope())
        {
            publishCommandResult.IsSuccess.Should().BeTrue();
            using (var session = _testFixture.OpenSession(_output))
            {
                var publishedPost = await session.GetAsync<Post>(publishCommandResult.Value, TestContext.Current.CancellationToken);
                publishedPost.Should().NotBeNull();
                _pictureStore.GetContent(PictureContainers.Posts, publishedPost.Picture).Should().Equal(command.PictureRawBytes);
            }
        }
    }

    [Fact]
    public async Task Should_Not_Publish_A_Post_Without_A_Picture()
    {
        var author = TestDataHelper.NewUser("nopictureauthor");
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(author, TestContext.Current.CancellationToken);
            await session.FlushAsync(TestContext.Current.CancellationToken);
        }

        var command = new PublishPostCommand(author.ID.Value, "post without a picture", []);
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = new TestMediatorBuilder(session).WithPictureStore(_pictureStore).Build();
            (await sut.Send(command, TestContext.Current.CancellationToken)).IsSuccess.Should().BeFalse();
        }
    }

    [Fact]
    public async Task Should_Put_a_Comment_On_A_Post()
    {
        Result<Guid> commentPostResult;
        var author = TestDataHelper.NewUser("author1");
        var commenter = TestDataHelper.NewUser("commenter1");
        Post post;
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(author, TestContext.Current.CancellationToken);
            await session.SaveAsync(commenter, TestContext.Current.CancellationToken);
            post = TestDataHelper.NewPost(author);
            await session.SaveAsync(post, TestContext.Current.CancellationToken);
            await session.FlushAsync(TestContext.Current.CancellationToken);
        }

        var command = new PublishCommentCommand(post.ID, "test comment", commenter.ID.Value);
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = new TestMediatorBuilder(session).Build();
            commentPostResult = await sut.Send(command, TestContext.Current.CancellationToken);
        }

        using (new AssertionScope())
        {
            commentPostResult.IsSuccess.Should().BeTrue();
            using (var session = _testFixture.OpenSession(_output))
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
        var author = TestDataHelper.NewUser("author9");
        var reader = TestDataHelper.NewUser("reader9");
        Post post;
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(author, TestContext.Current.CancellationToken);
            await session.SaveAsync(reader, TestContext.Current.CancellationToken);
            post = TestDataHelper.NewPost(author);
            await session.SaveAsync(post, TestContext.Current.CancellationToken);
            TestDataHelper.CommentOn(post, reader, "first comment");
            await session.FlushAsync(TestContext.Current.CancellationToken);
        }

        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = new TestMediatorBuilder(session).Build();
            commentPostResult = await sut.Send(new PublishCommentCommand(post.ID, "second comment", reader.ID.Value), TestContext.Current.CancellationToken);

            using (new AssertionScope())
            {
                commentPostResult.IsSuccess.Should().BeTrue();
                CommentsOf(session, post.ID).WasInitialized.Should().BeFalse("adding a comment does not need the other comments");
            }
        }

        using (var session = _testFixture.OpenSession(_output))
        {
            (await session.QueryOver<Comment>().Where(c => c.Post.ID == post.ID).RowCountAsync(TestContext.Current.CancellationToken)).Should().Be(2);
        }
    }

    [Fact]
    public async Task Should_Put_A_Like_To_A_Post()
    {
        Result likeResult;
        var author = TestDataHelper.NewUser("author2");
        var reader = TestDataHelper.NewUser("reader1");
        Post post;
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(author, TestContext.Current.CancellationToken);
            await session.SaveAsync(reader, TestContext.Current.CancellationToken);
            post = TestDataHelper.NewPost(author);
            await session.SaveAsync(post, TestContext.Current.CancellationToken);
            await session.FlushAsync(TestContext.Current.CancellationToken);
        }

        var command = new LikePostCommand(post.ID, reader.ID.Value);
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = new TestMediatorBuilder(session).Build();
            likeResult = await sut.Send(command, TestContext.Current.CancellationToken);
        }

        using (new AssertionScope())
        {
            likeResult.IsSuccess.Should().BeTrue();
            using (var session = _testFixture.OpenSession(_output))
            {
                (await session.QueryOver<Like>().Where(l => l.Post.ID == post.ID).RowCountAsync(TestContext.Current.CancellationToken)).Should().Be(1);
            }
        }
    }

    [Fact]
    public async Task A_Like_Put_Before_The_Post_Is_Saved_Should_Be_Stored_With_Its_Post()
    {
        var author = TestDataHelper.NewUser("author8");
        var reader = TestDataHelper.NewUser("reader8");
        Post post;
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(author, TestContext.Current.CancellationToken);
            await session.SaveAsync(reader, TestContext.Current.CancellationToken);
            post = TestDataHelper.NewPost(author);
            post.LikeBy(reader.ID.Value);
            await session.SaveAsync(post, TestContext.Current.CancellationToken);
            await session.FlushAsync(TestContext.Current.CancellationToken);
        }

        using (var session = _testFixture.OpenSession(_output))
        {
            var storedPost = await session.GetAsync<Post>(post.ID, TestContext.Current.CancellationToken);
            storedPost!.IsLikedBy(reader.ID.Value).Should().BeTrue();
        }
    }

    [Fact]
    public async Task Should_Dislike_A_Post()
    {
        Result likeResult;
        var author = TestDataHelper.NewUser("author3");
        var reader = TestDataHelper.NewUser("reader2");
        Post post;
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(author, TestContext.Current.CancellationToken);
            await session.SaveAsync(reader, TestContext.Current.CancellationToken);
            post = TestDataHelper.NewPost(author);
            await session.SaveAsync(post, TestContext.Current.CancellationToken);
            post.LikeBy(reader.ID.Value);
            await session.FlushAsync(TestContext.Current.CancellationToken);
        }

        var command = new DislikePostCommand(post.ID, reader.ID.Value);
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = new TestMediatorBuilder(session).Build();
            likeResult = await sut.Send(command, TestContext.Current.CancellationToken);
        }

        using (new AssertionScope())
        {
            likeResult.IsSuccess.Should().BeTrue();
            using (var session = _testFixture.OpenSession(_output))
            {
                (await session.QueryOver<Like>().Where(l => l.Post.ID == post.ID).RowCountAsync(TestContext.Current.CancellationToken)).Should().Be(0);
            }
        }
    }

    [Fact]
    public async Task Cannot_Dislike_A_Post_Without_Putting_A_Like_First()
    {
        Result likeResult;
        var author = TestDataHelper.NewUser("author4");
        var reader = TestDataHelper.NewUser("reader3");
        Post post;
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(author, TestContext.Current.CancellationToken);
            await session.SaveAsync(reader, TestContext.Current.CancellationToken);
            post = TestDataHelper.NewPost(author);
            await session.SaveAsync(post, TestContext.Current.CancellationToken);
            await session.FlushAsync(TestContext.Current.CancellationToken);
        }

        var command = new DislikePostCommand(post.ID, reader.ID.Value);
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = new TestMediatorBuilder(session).Build();
            likeResult = await sut.Send(command, TestContext.Current.CancellationToken);
        }

        likeResult.IsSuccess.Should().BeFalse("the reader has not liked this post");
    }

    [Fact]
    public async Task Cannot_Like_A_Post_Twice()
    {
        Result likeResult;
        var author = TestDataHelper.NewUser("author5");
        var reader = TestDataHelper.NewUser("reader4");
        Post post;
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(author, TestContext.Current.CancellationToken);
            await session.SaveAsync(reader, TestContext.Current.CancellationToken);
            post = TestDataHelper.NewPost(author);
            await session.SaveAsync(post, TestContext.Current.CancellationToken);
            await session.FlushAsync(TestContext.Current.CancellationToken);
        }

        var command = new LikePostCommand(post.ID, reader.ID.Value);
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = new TestMediatorBuilder(session).Build();
            await sut.Send(command, TestContext.Current.CancellationToken);
            likeResult = await sut.Send(command, TestContext.Current.CancellationToken);
        }

        likeResult.IsSuccess.Should().BeFalse("the reader has already liked this post");
    }

    [Fact]
    public async Task Author_Cannot_Put_Likes_On_Their_Own_Posts()
    {
        Result likeResult;
        var author = TestDataHelper.NewUser("author6");
        Post post;
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(author, TestContext.Current.CancellationToken);
            post = TestDataHelper.NewPost(author);
            await session.SaveAsync(post, TestContext.Current.CancellationToken);
            await session.FlushAsync(TestContext.Current.CancellationToken);
        }

        var command = new LikePostCommand(post.ID, author.ID.Value);
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = new TestMediatorBuilder(session).Build();
            likeResult = await sut.Send(command, TestContext.Current.CancellationToken);
        }

        likeResult.IsSuccess.Should().BeFalse("authors cannot like their own posts");
    }

    [Fact]
    public async Task Should_Find_One_Comment_For_Post()
    {
        CommentModel[] comments;
        var author = TestDataHelper.NewUser("author7");
        Post post;
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(author, TestContext.Current.CancellationToken);
            post = TestDataHelper.NewPost(author);
            await session.SaveAsync(post, TestContext.Current.CancellationToken);
            post.CommentBy(author.ID.Value, CommentText.Create("First comment").Value);
            await session.FlushAsync(TestContext.Current.CancellationToken);
        }

        var query = new PostCommentsQuery(post.ID);
        using (var session = _testFixture.OpenStatelessSession())
        {
            var sut = new PostCommentsQueryHandler(session, Log.Logger);
            comments = await sut.Handle(query, TestContext.Current.CancellationToken);
        }

        comments.Length.Should().Be(1);
    }

    [Fact]
    public async Task Should_Read_A_Post_For_Editing()
    {
        EditPostModel result;
        var author = TestDataHelper.NewUser("editingauthor");
        Post post;
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(author, TestContext.Current.CancellationToken);
            post = TestDataHelper.NewPost(author, "Text to edit");
            await session.SaveAsync(post, TestContext.Current.CancellationToken);
            await session.FlushAsync(TestContext.Current.CancellationToken);
        }

        var query = new EditPostQuery(post.ID, author.ID.Value);
        using (var session = _testFixture.OpenStatelessSession())
        {
            var sut = new EditPostQueryHandler(session, Log.Logger);
            result = await sut.Handle(query, TestContext.Current.CancellationToken);
        }

        using (new AssertionScope())
        {
            result.PostID.Should().Be(post.ID);
            result.AuthorID.Should().Be(author.ID.Value);
            result.Text.Should().Be("Text to edit");
            result.Picture.Should().Be(post.Picture);
        }
    }

    [Fact]
    public async Task Should_Load_All_Details_For_A_Post()
    {
        PostModel result;
        var author = TestDataHelper.NewUser("author7");
        var reader = TestDataHelper.NewUser("reader1");
        Post post;
        using (var session = _testFixture.OpenSession(_output))
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
        using (var session = _testFixture.OpenStatelessSession())
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
        PostModel[] timeline;
        var author1 = TestDataHelper.NewUser("authoruser1");
        var author2 = TestDataHelper.NewUser("authoruser2");
        var reader = TestDataHelper.NewUser("readeruser");
        Post post1;
        Post post2;
        Post post3;
        using (var session = _testFixture.OpenSession(_output))
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
            await session.SaveAsync(Follow.Create(reader.ID.Value, author1.ID.Value).Value, TestContext.Current.CancellationToken);
            TestDataHelper.CommentOn(post1, reader, "nice picture");
            await session.FlushAsync(TestContext.Current.CancellationToken);
            await session.FlushAsync(TestContext.Current.CancellationToken);
        }

        var query = new TimelineQuery(reader.ID.Value, 5);
        using (var session = _testFixture.OpenStatelessSession())
        {
            var sut = new TimelineQueryHandler(session, Log.Logger);
            timeline = await sut.Handle(query, TestContext.Current.CancellationToken);
        }

        using (new AssertionScope())
        {
            timeline.Length.Should().Be(2);
            timeline.Should().OnlyContain(p => p.AuthorNickName == author1.Nickname);
            var commentedPost = timeline.Single(p => p.PostID == post1.ID);
            commentedPost.Comments.Should().ContainSingle();
            commentedPost.Comments[0].AuthorNickName.Should().Be(reader.Nickname);
            commentedPost.Comments[0].Text.Should().Be("nice picture");
            timeline.Single(p => p.PostID == post2.ID).Comments.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task User_Can_Edit_His_Own_Posts()
    {
        Result editResult;
        var picture = await _pictureStore.SaveAsync(PictureContainers.Posts, Convert.FromBase64String(DatabaseFixture.GetTestPictureBase64()), TestContext.Current.CancellationToken);
        var author = TestDataHelper.NewUser("authoruser1");
        Post post;
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(author, TestContext.Current.CancellationToken);
            post = TestDataHelper.NewPost(author, "test post 1", picture);
            await session.SaveAsync(post, TestContext.Current.CancellationToken);
            await session.FlushAsync(TestContext.Current.CancellationToken);
        }

        var command = new EditPostCommand(author.ID.Value, post.ID, "edited text on post 1", Convert.FromBase64String(DatabaseFixture.GetTestPictureBase64()));
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = new TestMediatorBuilder(session).WithPictureStore(_pictureStore).Build();
            editResult = await sut.Send(command, TestContext.Current.CancellationToken);
        }

        using (var session = _testFixture.OpenSession(_output))
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
        Result editResult;
        var picture = PictureId.New();
        var author = TestDataHelper.NewUser("authoruser1");
        var reader = TestDataHelper.NewUser("readeruser");
        Post post;
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(author, TestContext.Current.CancellationToken);
            await session.SaveAsync(reader, TestContext.Current.CancellationToken);
            post = TestDataHelper.NewPost(author, "test post 1", picture);
            await session.SaveAsync(post, TestContext.Current.CancellationToken);
            await session.FlushAsync(TestContext.Current.CancellationToken);
        }

        var command = new EditPostCommand(reader.ID.Value, post.ID, "edited text on post 1", Convert.FromBase64String(DatabaseFixture.GetTestPictureBase64()));
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = new TestMediatorBuilder(session).WithPictureStore(_pictureStore).Build();
            editResult = await sut.Send(command, TestContext.Current.CancellationToken);
        }

        editResult.IsSuccess.Should().BeFalse($"You're not allowed to edit post {post.ID}.");
    }

    [Fact]
    public async Task Cannot_Edit_A_Post_That_Does_Not_Exist()
    {
        var missingPostID = PostId.New();
        var author = TestDataHelper.NewUser("authoruser2");
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(author, TestContext.Current.CancellationToken);
            await session.FlushAsync(TestContext.Current.CancellationToken);
        }

        var command = new EditPostCommand(author.ID.Value, missingPostID, "edited text", Convert.FromBase64String(DatabaseFixture.GetTestPictureBase64()));
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = new TestMediatorBuilder(session).WithPictureStore(_pictureStore).Build();
            var editResult = await sut.Send(command, TestContext.Current.CancellationToken);

            editResult.IsFailure.Should().BeTrue();
            editResult.Error.Should().Be($"Post {missingPostID} does not exist.");
        }
    }

    [Fact]
    public async Task A_New_Post_Should_Be_Stored_When_The_Session_Is_Flushed()
    {
        var author = TestDataHelper.NewUser("flushauthor");
        using var session = _testFixture.OpenSession(_output);
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
        var author = TestDataHelper.NewUser("insertauthor");
        var reader = TestDataHelper.NewUser("insertreader");
        var post = TestDataHelper.NewPost(author);
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(author, TestContext.Current.CancellationToken);
            await session.SaveAsync(reader, TestContext.Current.CancellationToken);
            await session.SaveAsync(post, TestContext.Current.CancellationToken);
            await session.FlushAsync(TestContext.Current.CancellationToken);
        }

        var statements = new XUnitSqlStatementOutputInterceptor(_output);
        using (var session = _testFixture.OpenSession(statements))
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

    private async Task<int> CountPostsAsync(PostId postID)
    {
        using var session = _testFixture.OpenStatelessSession();
        return await session.QueryOver<Post>()
            .Where(p => p.ID == postID)
            .RowCountAsync(TestContext.Current.CancellationToken);
    }

    private static IPersistentCollection CommentsOf(ISession session, PostId postID)
    {
        var role = $"{typeof(Post).FullName}.{nameof(Post.Comments)}";
        return session.GetSessionImplementation().PersistenceContext.CollectionEntries.Keys
            .OfType<IPersistentCollection>()
            .Single(c => c.Role == role && Equals(c.Key, postID));
    }
}
