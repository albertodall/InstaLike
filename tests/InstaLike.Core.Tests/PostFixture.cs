using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Posts;
using InstaLike.Core.SharedKernel;
using Xunit;

namespace InstaLike.Core.Tests;

public class PostFixture
{
    private const int PostAuthorID = 1;

    [Fact]
    public void Should_Comment_A_Post()
    {
        const int commentAuthorID = 42;
        var sut = new Post(PostAuthorID, PictureId.New(), (PostText)"test post");

        var comment = sut.CommentBy(commentAuthorID, (CommentText)"test comment");

        using (new AssertionScope())
        {
            sut.Comments.Should().ContainSingle().Which.Should().BeSameAs(comment);
            comment.Post.Should().BeSameAs(sut);
            comment.AuthorID.Should().Be(commentAuthorID);
            comment.Text.Should().Be((CommentText)"test comment");
        }
    }

    [Fact]
    public void Reader_Should_Like_A_Post()
    {
        const int readerID = 42;
        var sut = new Post(PostAuthorID, PictureId.New(), (PostText)"test post");

        var result = sut.LikeBy(readerID);

        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeTrue();
            sut.IsLikedBy(readerID).Should().BeTrue();
            var like = sut.Likes.Should().ContainSingle().Subject;
            like.Post.Should().BeSameAs(sut);
            like.UserID.Should().Be(readerID);
        }
    }

    [Fact]
    public void Liking_A_Post_Should_Raise_An_Event()
    {
        const int readerID = 42;
        var sut = new Post(PostAuthorID, PictureId.New(), (PostText)"test post");

        sut.LikeBy(readerID);

        sut.DomainEvents.Should().ContainSingle()
            .Which.Should().Be(new PostLikedEvent(sut.ID, readerID, PostAuthorID));
    }

    [Fact]
    public void Reader_Should_Not_Like_A_Post_Twice()
    {
        const int readerID = 42;
        var sut = new Post(PostAuthorID, PictureId.New(), (PostText)"test post");
        sut.LikeBy(readerID);

        var result = sut.LikeBy(readerID);

        using (new AssertionScope())
        {
            result.IsFailure.Should().BeTrue();
            sut.Likes.Should().ContainSingle();
            sut.DomainEvents.Should().ContainSingle();
        }
    }

    [Fact]
    public void Reader_Should_Dislike_A_Liked_Post()
    {
        const int readerID = 42;
        var sut = new Post(PostAuthorID, PictureId.New(), (PostText)"test post");
        sut.LikeBy(readerID);

        var result = sut.DislikeBy(readerID);

        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeTrue();
            sut.IsLikedBy(readerID).Should().BeFalse();
            sut.Likes.Should().BeEmpty();
        }
    }

    [Fact]
    public void Reader_Should_Not_Dislike_A_Post_Not_Liked()
    {
        var sut = new Post(PostAuthorID, PictureId.New(), (PostText)"test post");

        sut.DislikeBy(42).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Commenting_A_Post_Should_Raise_An_Event()
    {
        const int commentAuthorID = 42;
        var sut = new Post(PostAuthorID, PictureId.New(), (PostText)"test post");

        sut.CommentBy(commentAuthorID, (CommentText)"test comment");

        sut.DomainEvents.Should().ContainSingle()
            .Which.Should().Be(new CommentPublishedEvent(sut.ID, commentAuthorID, PostAuthorID));
    }

    [Fact]
    public void Post_Author_Should_Not_Put_Like_To_His_Posts()
    {
        var sut = new Post(PostAuthorID, PictureId.New(), (PostText)"test post");

        using (new AssertionScope())
        {
            sut.LikeBy(PostAuthorID).IsSuccess
                .Should()
                .BeFalse("users cannot put a 'Like' on their own posts");
            sut.DomainEvents.Should().BeEmpty();
        }
    }

    [Fact]
    public void Author_Can_Edit_His_Own_Post()
    {
        var sut = new Post(PostAuthorID, PictureId.New(), (PostText)"test post");

        sut.CanBeEditedBy(PostAuthorID).Should().BeTrue();
    }

    [Fact]
    public void User_Cannot_Edit_Post_Authored_By_Other_Users()
    {
        var sut = new Post(PostAuthorID, PictureId.New(), (PostText)"test post");

        sut.CanBeEditedBy(PostAuthorID + 1).Should().BeFalse();
    }
}
