using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Accounts;
using InstaLike.Core.Posts;
using InstaLike.Core.SharedKernel;
using Xunit;

namespace InstaLike.Core.Tests;

public class PostFixture
{
    [Fact]
    public void Should_Add_Comment_To_Post()
    {
        var postAuthor = CreateTestPostAuthor();
        var commentAuthor = new User(
            (Nickname)"user2",
            (FullName)"Test User 2",
            (Password)"password",
            (Email)"user2@acme.com",
            "My Bio");
        var sut = new Post(postAuthor, PictureId.New(), (PostText)"test post");
        var comment = new Comment(sut, commentAuthor, (CommentText)"test comment");

        sut.AddComment(comment);

        sut.Comments.Count.Should().Be(1);
    }

    [Fact]
    public void Reader_Should_Like_A_Post()
    {
        const int readerID = 42;
        var sut = new Post(CreateTestPostAuthor(), PictureId.New(), (PostText)"test post");

        var like = sut.LikeBy(readerID);

        using (new AssertionScope())
        {
            like.IsSuccess.Should().BeTrue();
            like.Value.PostID.Should().Be(sut.ID);
            like.Value.UserID.Should().Be(readerID);
        }
    }

    [Fact]
    public void Post_Author_Should_Not_Put_Like_To_His_Posts()
    {
        var postAuthor = CreateTestPostAuthor();
        var sut = new Post(postAuthor, PictureId.New(), (PostText)"test post");

        sut.LikeBy(postAuthor.ID).IsSuccess
            .Should()
            .BeFalse("users cannot put a 'Like' on their own posts");
    }

    [Fact]
    public void Author_Can_Edit_His_Own_Post()
    {
        var postAuthor = CreateTestPostAuthor();
        var sut = new Post(postAuthor, PictureId.New(), (PostText)"test post");

        sut.CanBeEditedBy(postAuthor).Should().BeTrue();
    }

    [Fact]
    public void User_Cannot_Edit_Post_Authored_By_Other_Users()
    {
        var postAuthor = CreateTestPostAuthor();
        var sut = new Post(postAuthor, PictureId.New(), (PostText)"test post");

        var otherUser = new User(
            (Nickname)"user2",
            (FullName)"Test User 2",
            (Password)"password",
            (Email)"user2@acme.com",
            "My Bio");

        sut.CanBeEditedBy(otherUser).Should().BeFalse();
    }

    private static User CreateTestPostAuthor()
    {
        return new User(
            (Nickname)"user1",
            (FullName)"Test User",
            (Password)"password",
            (Email)"user1@acme.com",
            "My Bio");
    }
}
