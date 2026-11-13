using InstaLike.Core.Accounts;
using InstaLike.Core.Membership;
using InstaLike.Core.Posts;
using InstaLike.Core.SharedKernel;

namespace InstaLike.IntegrationTests;

internal static class TestDataHelper
{
    private const string DefaultPassword = "password";

    public static Member NewMember(string nickname)
    {
        return NewMember(nickname, $"{nickname}@acme.com");
    }

    public static Member NewMember(string nickname, string email)
    {
        return CreateMember(nickname, email, DefaultPassword);
    }

    public static Member NewMemberWithPassword(string nickname, string password)
    {
        return CreateMember(nickname, $"{nickname}@acme.com", password);
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

    private static Member CreateMember(string nickname, string email, string password)
    {
        return new Member((Nickname)nickname, (FullName)$"{nickname} user", Password.Create(password).Value, (Email)email, "my bio");
    }
}
