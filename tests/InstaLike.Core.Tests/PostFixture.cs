using System;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Posts;
using InstaLike.Core.SharedKernel;
using Xunit;

namespace InstaLike.Core.Tests;

public sealed class PostFixture
{
    private static readonly Guid PostAuthorID = Guid.NewGuid();

    [Fact]
    public void A_New_Post_Should_Be_Identified_Before_It_Is_Stored()
    {
        var sut = new Post(PostAuthorID, PictureId.New(), (PostText)"test post");

        sut.ID.Value.Should().NotBeEmpty();
    }

    [Fact]
    public void Should_Comment_A_Post()
    {
        var commentAuthorID = Guid.NewGuid();
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
        var readerID = Guid.NewGuid();
        var sut = new Post(PostAuthorID, PictureId.New(), (PostText)"test post");

        var result = sut.LikeBy(readerID);

        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeTrue();
            sut.IsLikedBy(readerID).Should().BeTrue();
            var like = sut.Likes.Should().ContainSingle().Subject;
            like.Post.Should().BeSameAs(sut);
            like.MemberID.Should().Be(readerID);
        }
    }

    [Fact]
    public void Liking_A_Post_Should_Raise_An_Event()
    {
        var readerID = Guid.NewGuid();
        var sut = new Post(PostAuthorID, PictureId.New(), (PostText)"test post");

        sut.LikeBy(readerID);

        sut.DomainEvents.Should().ContainSingle()
            .Which.Should().Be(new PostLikedEvent(sut.ID, readerID, PostAuthorID));
    }

    [Fact]
    public void Reader_Should_Not_Like_A_Post_Twice()
    {
        var readerID = Guid.NewGuid();
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
        var readerID = Guid.NewGuid();
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

        sut.DislikeBy(Guid.NewGuid()).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Commenting_A_Post_Should_Raise_An_Event()
    {
        var commentAuthorID = Guid.NewGuid();
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
                .BeFalse("authors cannot like their own posts");
            sut.DomainEvents.Should().BeEmpty();
        }
    }

    [Fact]
    public void Reader_Can_Like_A_Post_But_Its_Author_Cannot()
    {
        var sut = new Post(PostAuthorID, PictureId.New(), (PostText)"test post");

        using (new AssertionScope())
        {
            sut.CanBeLikedBy(Guid.NewGuid()).Should().BeTrue();
            sut.CanBeLikedBy(PostAuthorID).Should().BeFalse();
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

        sut.CanBeEditedBy(Guid.NewGuid()).Should().BeFalse();
    }
}
