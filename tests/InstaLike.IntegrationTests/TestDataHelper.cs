using InstaLike.Core.Accounts;
using InstaLike.Core.Membership;
using InstaLike.Core.Posts;
using InstaLike.Core.SharedKernel;
using InstaLike.Core.Social;

namespace InstaLike.IntegrationTests
{
    internal static class TestDataHelper
    {
        private const string DefaultPassword = "password";

        public static Member NewMember(string nickname)
        {
            return Member.Register(
                (Nickname)nickname,
                (FullName)$"{nickname} user",
                "my bio",
                (Username)nickname,
                Password.Create(DefaultPassword).Value,
                (Email)$"{nickname}@acme.com");
        }

        public static Account NewAccount(Member member)
        {
            return NewAccountWithPassword(member, DefaultPassword);
        }

        public static Account NewAccountWithPassword(Member member, string password)
        {
            return Account.Create(member.ID.Value, (Username)member.Nickname.Value, Password.Create(password).Value, (Email)$"{member.Nickname.Value}@acme.com");
        }

        public static Account NewAccountWithEmail(Member member, string email)
        {
            return Account.Create(member.ID.Value, (Username)member.Nickname.Value, Password.Create(DefaultPassword).Value, (Email)email);
        }

        public static Post NewPost(Member author)
        {
            return NewPost(author, "test post");
        }

        public static Post NewPost(Member author, string text)
        {
            return NewPost(author, text, PictureId.New());
        }

        public static Post NewPost(Member author, string text, PictureId picture)
        {
            return new Post(author.ID.Value, picture, (PostText)text);
        }

        public static Comment CommentOn(Post post, Member author, string text)
        {
            return post.CommentBy(author.ID.Value, (CommentText)text);
        }

        public static Follower Following(Member member, Member followed)
        {
            var follower = new Follower(member.ID.Value);
            follower.Follow(followed.ID.Value);
            return follower;
        }
    }
}
