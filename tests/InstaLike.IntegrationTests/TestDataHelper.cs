using InstaLike.Core.Accounts;
using InstaLike.Core.Membership;
using InstaLike.Core.Posts;
using InstaLike.Core.SharedKernel;

namespace InstaLike.IntegrationTests;

internal static class TestDataHelper
{
    private const string DefaultPassword = "password";

    /// <summary>
    /// A newly registered member, using the nickname as username and the default password.
    /// </summary>
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

    /// <summary>
    /// The member's account, with the default password.
    /// </summary>
    public static Account NewAccount(Member member)
    {
        return NewAccountWithPassword(member, DefaultPassword);
    }

    /// <summary>
    /// The member's account, with the given password.
    /// </summary>
    public static Account NewAccountWithPassword(Member member, string password)
    {
        return Account.Create(member.ID.Value, (Username)member.Nickname.Value, Password.Create(password).Value, (Email)$"{member.Nickname.Value}@acme.com");
    }

    /// <summary>
    /// The member's account, with the given email address.
    /// </summary>
    public static Account NewAccountWithEmail(Member member, string email)
    {
        return Account.Create(member.ID.Value, (Username)member.Nickname.Value, Password.Create(DefaultPassword).Value, (Email)email);
    }

    /// <summary>
    /// A post by the author, with a default text and a new picture.
    /// </summary>
    public static Post NewPost(Member author)
    {
        return NewPost(author, "test post");
    }

    /// <summary>
    /// A post by the author with the given text and a new picture.
    /// </summary>
    public static Post NewPost(Member author, string text)
    {
        return NewPost(author, text, PictureId.New());
    }

    /// <summary>
    /// A post by the author with the given text and picture.
    /// </summary>
    public static Post NewPost(Member author, string text, PictureId picture)
    {
        return new Post(author.ID.Value, picture, (PostText)text);
    }

    /// <summary>
    /// The author's comment on the post.
    /// </summary>
    public static Comment CommentOn(Post post, Member author, string text)
    {
        return post.CommentBy(author.ID.Value, (CommentText)text);
    }
}
