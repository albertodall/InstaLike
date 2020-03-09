using System;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Domain;
using InstaLike.Core.Tests.Properties;
using Xunit;

namespace InstaLike.Core.Tests
{
    public class PostFixture
    {
        private readonly string _testPictureBase64 = Convert.ToBase64String(Resources.GrumpyCat);

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
            var sut = new Post(postAuthor, (Picture)_testPictureBase64, (PostText)"test post");
            var comment = new Comment(sut, commentAuthor, (CommentText)"test comment");

            sut.AddComment(comment);

            sut.Comments.Count.Should().Be(1);
        }

        [Fact]
        public void Adding_Null_Comment_To_Post_Should_Throw_Exception()
        {
            var postAuthor = CreateTestPostAuthor();
            var sut = new Post(postAuthor, (Picture)_testPictureBase64, (PostText)"test post");

            sut.Invoking(obj => obj.AddComment(null))
                .Should()
                .Throw<ArgumentNullException>();
        }

        [Fact]
        public void Should_Add_Like_To_Post()
        {
            var postAuthor = CreateTestPostAuthor();
            var postReader = new User(
                (Nickname)"user2",
                (FullName)"Test User 2",
                (Password)"password",
                (Email)"user2@acme.com",
                "My Bio");
            var sut = new Post(postAuthor, (Picture)_testPictureBase64, (PostText)"test post");

            sut.PutLikeBy(postReader);

            using (new AssertionScope())
            {
                sut.LikesTo(postReader).Should().BeTrue();
                sut.Likes.Count.Should().Be(1);
            }
        }

        [Fact]
        public void Null_User_Should_Not_Put_Like_To_Post()
        {
            var postAuthor = CreateTestPostAuthor();
            var sut = new Post(postAuthor, (Picture)_testPictureBase64, (PostText)"test post");

            sut.Invoking(obj => obj.PutLikeBy(null))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Post_Author_Should_Not_Put_Like_To_His_Posts()
         {
            var postAuthor = CreateTestPostAuthor();
            var sut = new Post(postAuthor, (Picture)_testPictureBase64, (PostText)"test post");

            sut.PutLikeBy(postAuthor).IsSuccess
                .Should()
                .BeFalse($"User [{postAuthor.Nickname}] cannot put a 'Like' on their own posts.");
        }

        [Fact]
        public void Cannot_Put_Like_To_Post_Twice()
        {
            var postAuthor = CreateTestPostAuthor();
            var postReader = new User(
                (Nickname)"user2",
                (FullName)"Test User 2",
                (Password)"password",
                (Email)"user2@acme.com",
                "My Bio");
            var sut = new Post(postAuthor, (Picture)_testPictureBase64, (PostText)"test post");

            using (new AssertionScope())
            {
                sut.PutLikeBy(postReader).IsSuccess
                    .Should()
                    .BeTrue();
                sut.PutLikeBy(postReader).IsSuccess
                    .Should()
                    .BeFalse($"User [{postReader.Nickname}] has already put a 'Like' to this post.");
            }
        }

        [Fact]
        public void Should_Remove_Like_From_Post()
        {
            var postAuthor = CreateTestPostAuthor();
            var postReader = new User(
                (Nickname)"user2",
                (FullName)"Test User 2",
                (Password)"password",
                (Email)"user2@acme.com",
                "My Bio");
            var sut = new Post(postAuthor, (Picture)_testPictureBase64, (PostText)"test post");

            sut.PutLikeBy(postReader);

            using (new AssertionScope())
            {
                sut.RemoveLikeBy(postReader).IsSuccess.Should().BeTrue();
                sut.LikesTo(postReader).Should().BeFalse();
                sut.Likes.Count().Should().Be(0);
            }
        }

        [Fact]
        public void Should_Not_Remove_Like_From_Post_If_Not_Liked_Before()
        {
            var postAuthor = CreateTestPostAuthor();
            var postReader = new User(
                (Nickname)"user2",
                (FullName)"Test User 2",
                (Password)"password",
                (Email)"user2@acme.com",
                "My Bio");
            var sut = new Post(postAuthor, (Picture)_testPictureBase64, (PostText)"test post");

            sut.RemoveLikeBy(postReader).IsSuccess
                .Should()
                .BeFalse($"User [{postReader.Nickname}] did not put any 'Like' on this post.");
        }

        [Fact]
        public void Null_User_Cannot_Put_Like_To_Post()
        {
            var postAuthor = CreateTestPostAuthor();
            var sut = new Post(postAuthor, (Picture)_testPictureBase64, (PostText)"test post");

            sut.Invoking(obj => obj.PutLikeBy(null))
                .Should()
                .Throw<ArgumentNullException>();
        }

        [Fact]
        public void Null_User_Cannot_Remove_Like_From_Post()
        {
            var postAuthor = CreateTestPostAuthor();
            var sut = new Post(postAuthor, (Picture)_testPictureBase64, (PostText)"test post");

            sut.Invoking(obj => obj.RemoveLikeBy(null))
                .Should()
                .Throw<ArgumentNullException>();
        }

        [Fact]
        public void Post_Should_Be_Liked_By_User()
        {
            var postAuthor = CreateTestPostAuthor();
            var postReader = new User(
                (Nickname)"user2",
                (FullName)"Test User 2",
                (Password)"password",
                (Email)"user2@acme.com",
                "My Bio");
            var sut = new Post(postAuthor, (Picture)_testPictureBase64, (PostText)"test post");

            sut.PutLikeBy(postReader);

            sut.LikesTo(postReader).Should().BeTrue();
        }

        [Fact]
        public void Post_Should_Not_Be_Liked_By_User_Without_Putting_A_Like_First()
        {
            var postAuthor = CreateTestPostAuthor();
            var postReader = new User(
                (Nickname)"user2",
                (FullName)"Test User 2",
                (Password)"password",
                (Email)"user2@acme.com",
                "My Bio");
            var sut = new Post(postAuthor, (Picture)_testPictureBase64, (PostText)"test post");

            sut.LikesTo(postReader).Should().BeFalse();
        }

        [Fact]
        public void Checking_Like_For_Null_User_Should_Throw_Exception()
        {
            var postAuthor = CreateTestPostAuthor();
            var sut = new Post(postAuthor, (Picture)_testPictureBase64, (PostText)"test post");

            sut.Invoking(obj => obj.LikesTo(null))
                .Should()
                .Throw<ArgumentNullException>();
        }

        [Fact]
        public void Author_Can_Edit_His_Own_Post()
        {
            var postAuthor = CreateTestPostAuthor();
            var sut = new Post(postAuthor, (Picture)_testPictureBase64, (PostText)"test post");

            sut.CanBeEditedBy(postAuthor).Should().BeTrue();
        }

        [Fact]
        public void User_Cannot_Edit_Post_Authored_By_Other_Users()
        {
            var postAuthor = CreateTestPostAuthor();
            var sut = new Post(postAuthor, (Picture)_testPictureBase64, (PostText)"test post");

            var otherUser = new User(
                (Nickname)"user2", 
                (FullName)"Test User 2", 
                (Password)"password", 
                (Email)"user2@acme.com", 
                "My Bio");

            sut.CanBeEditedBy(otherUser).Should().BeFalse();
        }

        [Fact]
        public void Null_User_Cannot_Edit_Post()
        {
            var postAuthor = CreateTestPostAuthor();
            var sut = new Post(postAuthor, (Picture)_testPictureBase64, (PostText)"test post");

            sut.Invoking(post => post.CanBeEditedBy(null))
                .Should()
                .Throw<ArgumentNullException>();
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
}