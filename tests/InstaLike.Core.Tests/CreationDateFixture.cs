using System;
using FluentAssertions;
using InstaLike.Core.Accounts;
using InstaLike.Core.Membership;
using InstaLike.Core.Notifications;
using InstaLike.Core.Posts;
using InstaLike.Core.SharedKernel;
using InstaLike.Core.Social;
using Xunit;

namespace InstaLike.Core.Tests
{
    public sealed class CreationDateFixture
    {
        private static readonly Guid AuthorID = Guid.NewGuid();
        private static readonly Guid ReaderID = Guid.NewGuid();

        [Fact]
        public void A_Member_Registers_In_Utc()
        {
            var member = Member.Register(
                (Nickname)"user1",
                (FullName)"Test User",
                "My Bio",
                (Username)"user1",
                Password.Create("password").Value,
                (Email)"user1@acme.com");

            member.RegistrationDate.Offset.Should().Be(TimeSpan.Zero);
        }

        [Fact]
        public void A_Post_Is_Published_In_Utc()
        {
            CreatePost().PostDate.Offset.Should().Be(TimeSpan.Zero);
        }

        [Fact]
        public void A_Comment_Is_Written_In_Utc()
        {
            var comment = CreatePost().CommentBy(ReaderID, (CommentText)"test comment");

            comment.CommentDate.Offset.Should().Be(TimeSpan.Zero);
        }

        [Fact]
        public void A_Like_Is_Put_In_Utc()
        {
            var post = CreatePost();

            post.LikeBy(ReaderID);

            post.Likes.Should().ContainSingle()
                .Which.LikeDate.Offset.Should().Be(TimeSpan.Zero);
        }

        [Fact]
        public void A_Follow_Starts_In_Utc()
        {
            var follower = new Follower(ReaderID);

            follower.Follow(AuthorID);

            follower.Follows.Should().ContainSingle()
                .Which.FollowDate.Offset.Should().Be(TimeSpan.Zero);
        }

        [Fact]
        public void A_Notification_Is_Sent_In_Utc()
        {
            var notification = Notification.Followed(ReaderID, AuthorID);

            notification.NotificationDate.Offset.Should().Be(TimeSpan.Zero);
        }

        /// <summary>
        /// A new post by the test author.
        /// </summary>
        private static Post CreatePost()
        {
            return new Post(AuthorID, PictureId.New(), (PostText)"test post");
        }
    }
}
