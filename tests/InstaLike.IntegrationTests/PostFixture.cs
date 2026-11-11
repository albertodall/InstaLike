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
using InstaLike.Web.Posts.Queries;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace InstaLike.IntegrationTests;

public class PostFixture : IClassFixture<DatabaseFixture>
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
        const string testPassword = "password";
        Result<int> publishCommandResult;

        var testUser = new User((Nickname)testUserName, (FullName)"test1 user1", Password.Create(testPassword).Value, (Email)"testuser1@acme.com", "bio1");
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(testUser);
        }

        var command = new PublishPostCommand(testUser.ID, "test post 1", Convert.FromBase64String(DatabaseFixture.GetTestPictureBase64()));
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = DatabaseFixture.CreateMediator(session, _pictureStore);
            publishCommandResult = await sut.Send(command, default);
        }

        using (new AssertionScope())
        {
            publishCommandResult.IsSuccess.Should().BeTrue();
            using (var session = _testFixture.OpenSession(_output))
            {
                var publishedPost = await session.GetAsync<Post>(publishCommandResult.Value);
                publishedPost.Should().NotBeNull();
                _pictureStore.GetContent(PictureContainers.Posts, publishedPost.Picture).Should().Equal(command.PictureRawBytes);
            }
        }
    }

    [Fact]
    public async Task Should_Not_Publish_A_Post_Without_A_Picture()
    {
        var author = new User((Nickname)"nopictureauthor", (FullName)"nopicture author", Password.Create("password").Value, (Email)"nopictureauthor@acme.com", "bio");
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(author);
        }

        var command = new PublishPostCommand(author.ID, "post without a picture", []);
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = DatabaseFixture.CreateMediator(session, _pictureStore);
            (await sut.Send(command, default)).IsSuccess.Should().BeFalse();
        }
    }

    [Fact]
    public async Task Should_Put_a_Comment_On_A_Post()
    {
        Result<int> commentPostResult;
        var author = new User((Nickname)"author1", (FullName)"author one", Password.Create("password").Value, (Email)"author1@acme.com", "my bio");
        var commenter = new User((Nickname)"commenter1", (FullName)"commenter one", Password.Create("password").Value, (Email)"commenter1@acme.com", "my bio");
        var post = new Post(author, PictureId.New(), (PostText)"test post");
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(author);
            await session.SaveAsync(commenter);
            await session.SaveAsync(post);
        }

        var command = new PublishCommentCommand(post.ID, "test comment", commenter.ID);
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = DatabaseFixture.CreateMediator(session);
            commentPostResult = await sut.Send(command, default);
        }

        using (new AssertionScope())
        {
            commentPostResult.IsSuccess.Should().BeTrue();
            using (var session = _testFixture.OpenSession(_output))
            {
                (await session.GetAsync<Comment>(commentPostResult.Value)).Should().NotBeNull();
                (await session.QueryOver<Comment>().Where(c => c.PostID == post.ID).RowCountAsync()).Should().Be(1);
            }
        }
    }

    [Fact]
    public async Task Should_Put_A_Like_To_A_Post()
    {
        Result likeResult;
        var author = new User((Nickname)"author2", (FullName)"author two", Password.Create("password").Value, (Email)"author2@acme.com", "my bio");
        var reader = new User((Nickname)"reader1", (FullName)"reader one", Password.Create("password").Value, (Email)"reader1@acme.com", "my bio");
        var post = new Post(author, PictureId.New(), (PostText)"test post");
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(author);
            await session.SaveAsync(reader);
            await session.SaveAsync(post);
        }

        var command = new LikePostCommand(post.ID, reader.ID);
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = DatabaseFixture.CreateMediator(session);
            likeResult = await sut.Send(command, default);
        }

        using (new AssertionScope())
        {
            likeResult.IsSuccess.Should().BeTrue();
            using (var session = _testFixture.OpenSession(_output))
            {
                (await session.QueryOver<Like>().Where(l => l.PostID == post.ID).RowCountAsync()).Should().Be(1);
            }
        }
    }

    [Fact]
    public async Task Should_Dislike_A_Post()
    {
        Result likeResult;
        var author = new User((Nickname)"author3", (FullName)"author three", Password.Create("password").Value, (Email)"author3@acme.com", "my bio");
        var reader = new User((Nickname)"reader2", (FullName)"reader two", Password.Create("password").Value, (Email)"reader2@acme.com", "my bio");
        var post = new Post(author, PictureId.New(), (PostText)"test post");
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(author);
            await session.SaveAsync(reader);
            await session.SaveAsync(post);
            await session.SaveAsync(post.LikeBy(reader.ID).Value);
        }

        var command = new DislikePostCommand(post.ID, reader.ID);
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = DatabaseFixture.CreateMediator(session);
            likeResult = await sut.Send(command, default);
        }

        using (new AssertionScope())
        {
            likeResult.IsSuccess.Should().BeTrue();
            using (var session = _testFixture.OpenSession(_output))
            {
                (await session.QueryOver<Like>().Where(l => l.PostID == post.ID).RowCountAsync()).Should().Be(0);
            }
        }
    }

    [Fact]
    public async Task Cannot_Dislike_A_Post_Without_Putting_A_Like_First()
    {
        Result likeResult;
        var author = new User((Nickname)"author4", (FullName)"author four", Password.Create("password").Value, (Email)"author4@acme.com", "my bio");
        var reader = new User((Nickname)"reader3", (FullName)"reader three", Password.Create("password").Value, (Email)"reader3@acme.com", "my bio");
        var post = new Post(author, PictureId.New(), (PostText)"test post");
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(author);
            await session.SaveAsync(reader);
            await session.SaveAsync(post);
        }

        var command = new DislikePostCommand(post.ID, reader.ID);
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = DatabaseFixture.CreateMediator(session);
            likeResult = await sut.Send(command, default);
        }

        likeResult.IsSuccess.Should().BeFalse($"User [{reader.Nickname}] did not put any 'Like' on this post.");
    }

    [Fact]
    public async Task Cannot_Like_A_Post_Twice()
    {
        Result likeResult;
        var author = new User((Nickname)"author5", (FullName)"author five", Password.Create("password").Value, (Email)"author5@acme.com", "my bio");
        var reader = new User((Nickname)"reader4", (FullName)"reader four", Password.Create("password").Value, (Email)"reader4@acme.com", "my bio");
        var post = new Post(author, PictureId.New(), (PostText)"test post");
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(author);
            await session.SaveAsync(reader);
            await session.SaveAsync(post);
        }

        var command = new LikePostCommand(post.ID, reader.ID);
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = DatabaseFixture.CreateMediator(session);
            await sut.Send(command, default);
            likeResult = await sut.Send(command, default);
        }

        likeResult.IsSuccess.Should().BeFalse($"User [{reader.Nickname}] already 'Liked' this post.");
    }

    [Fact]
    public async Task Author_Cannot_Put_Likes_On_Their_Own_Posts()
    {
        Result likeResult;
        var author = new User((Nickname)"author6", (FullName)"author six", Password.Create("password").Value, (Email)"author6@acme.com", "my bio");
        var post = new Post(author, PictureId.New(), (PostText)"test post");
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(author);
            await session.SaveAsync(post);
        }

        var command = new LikePostCommand(post.ID, author.ID);
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = DatabaseFixture.CreateMediator(session);
            likeResult = await sut.Send(command, default);
        }

        likeResult.IsSuccess.Should().BeFalse("You cannot put a 'Like' on your own posts.");
    }

    [Fact]
    public async Task Should_Find_One_Comment_For_Post()
    {
        CommentModel[] comments;
        var author = new User((Nickname)"author7", (FullName)"author seven", Password.Create("password").Value, (Email)"author7@acme.com", "my bio");
        var post = new Post(author, PictureId.New(), (PostText)"test post");
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(author);
            await session.SaveAsync(post);
            await session.SaveAsync(post.CommentBy(author.ID, CommentText.Create("First comment").Value));
        }

        var query = new PostCommentsQuery(post.ID);
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = new PostCommentsQueryHandler(session, Log.Logger);
            comments = await sut.Handle(query, default);
        }

        comments.Length.Should().Be(1);
    }

    [Fact]
    public async Task Should_Load_All_Details_For_A_Post()
    {
        PostModel result;
        var author = new User((Nickname)"author7", (FullName)"author seven", Password.Create("password").Value, (Email)"author7@acme.com", "my bio");
        var reader = new User((Nickname)"reader1", (FullName)"reader one", Password.Create("password").Value, (Email)"reader1@acme.com", "my bio");
        var post = new Post(author, PictureId.New(), (PostText)"test post");
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(reader);
            await session.SaveAsync(author);
            await session.SaveAsync(post);
            await session.SaveAsync(post.CommentBy(reader.ID, CommentText.Create("My comment").Value));
            await session.SaveAsync(post.LikeBy(reader.ID).Value);
        }

        var query = new PostDetailQuery(post.ID, reader.ID);
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = new PostDetailQueryHandler(session, Log.Logger);
            result = await sut.Handle(query, default);
        }

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.LikesCount.Should().Be(1);
            result.Comments.Length.Should().Be(1);
            result.IsLikedByCurrentUser.Should().BeTrue();
        }
    }

    [Fact]
    public async Task Should_Read_Posts_Created_By_Followed_Users()
    {
        PostModel[] timeline;
        var author1 = new User((Nickname)"authoruser1", (FullName)"author1 user1", Password.Create("password").Value, (Email)"author1user1@acme.com", "my bio");
        var author2 = new User((Nickname)"authoruser2", (FullName)"author2 user2", Password.Create("password").Value, (Email)"author2user2@acme.com", "my bio");
        var reader = new User((Nickname)"readeruser", (FullName)"reader user", Password.Create("password").Value, (Email)"reader@acme.com", "my bio");
        var post1 = new Post(author1, PictureId.New(), (PostText)"test post 1");
        var post2 = new Post(author1, PictureId.New(), (PostText)"test post 2");
        var post3 = new Post(author2, PictureId.New(), (PostText)"test post 3");
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(author1);
            await session.SaveAsync(author2);
            await session.SaveAsync(reader);
            await session.SaveAsync(post1);
            await session.SaveAsync(post2);
            await session.SaveAsync(post3);
            await session.SaveAsync(Follow.Create(reader.ID, author1.ID).Value);
            await session.SaveAsync(post1.CommentBy(reader.ID, (CommentText)"nice picture"));
            await session.FlushAsync();
        }

        var query = new TimelineQuery(reader.ID, 5);
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = new TimelineQueryHandler(session, Log.Logger);
            timeline = await sut.Handle(query, default);
        }

        using (new AssertionScope())
        {
            timeline.Length.Should().Be(2);
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
        var picture = await _pictureStore.SaveAsync(PictureContainers.Posts, Convert.FromBase64String(DatabaseFixture.GetTestPictureBase64()), default);
        var author = new User((Nickname)"authoruser1", (FullName)"author1 user1", Password.Create("password").Value, (Email)"author1user1@acme.com", "my bio");
        var post = new Post(author, picture, (PostText)"test post 1");
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(author);
            await session.SaveAsync(post);
        }

        var command = new EditPostCommand(author.ID, post.ID, "edited text on post 1", Convert.FromBase64String(DatabaseFixture.GetTestPictureBase64()));
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = DatabaseFixture.CreateMediator(session, _pictureStore);
            editResult = await sut.Send(command, default);
        }

        using (var session = _testFixture.OpenSession(_output))
        {
            using (new AssertionScope())
            {
                var editedPost = session.Load<Post>(post.ID);
                editedPost.Text.Value.Should().Be("edited text on post 1");
                editedPost.Picture.Should().NotBe(post.Picture, "stored pictures are never overwritten");
                _pictureStore.GetContent(PictureContainers.Posts, picture).Should().BeNull("the replaced picture is deleted after the commit");
                editResult.IsSuccess.Should().BeTrue();
            }
        }
    }

    [Fact]
    public async Task User_Cannot_Edit_Post_Published_By_Another_User()
    {
        Result editResult;
        var picture = PictureId.New();
        var author = new User((Nickname)"authoruser1", (FullName)"author1 user1", Password.Create("password").Value, (Email)"author1user1@acme.com", "my bio");
        var reader = new User((Nickname)"readeruser", (FullName)"reader user", Password.Create("password").Value, (Email)"reader@acme.com", "my bio");
        var post = new Post(author, picture, (PostText)"test post 1");
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(author);
            await session.SaveAsync(reader);
            await session.SaveAsync(post);
        }

        var command = new EditPostCommand(reader.ID, post.ID, "edited text on post 1", Convert.FromBase64String(DatabaseFixture.GetTestPictureBase64()));
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = DatabaseFixture.CreateMediator(session, _pictureStore);
            editResult = await sut.Send(command, default);
        }

        editResult.IsSuccess.Should().BeFalse($"You're not allowed to edit post {post.ID}.");
    }

    [Fact]
    public async Task Cannot_Edit_A_Post_That_Does_Not_Exist()
    {
        const int missingPostID = int.MaxValue;
        var author = new User((Nickname)"authoruser2", (FullName)"author2 user2", Password.Create("password").Value, (Email)"author2user2@acme.com", "my bio");
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(author);
        }

        var command = new EditPostCommand(author.ID, missingPostID, "edited text", Convert.FromBase64String(DatabaseFixture.GetTestPictureBase64()));
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = DatabaseFixture.CreateMediator(session, _pictureStore);
            var editResult = await sut.Send(command, default);

            editResult.IsFailure.Should().BeTrue();
            editResult.Error.Should().Be($"Post {missingPostID} does not exist.");
        }
    }
}
