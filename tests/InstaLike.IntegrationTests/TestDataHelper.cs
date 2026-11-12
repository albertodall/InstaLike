using InstaLike.Core.Accounts;
using InstaLike.Core.Posts;
using InstaLike.Core.SharedKernel;

namespace InstaLike.IntegrationTests;

internal static class TestDataHelper
{
    private const string DefaultPassword = "password";

    public static User NewUser(string nickname)
    {
        return NewUser(nickname, $"{nickname}@acme.com");
    }

    public static User NewUser(string nickname, string email)
    {
        return CreateUser(nickname, email, DefaultPassword);
    }

    public static User NewUserWithPassword(string nickname, string password)
    {
        return CreateUser(nickname, $"{nickname}@acme.com", password);
    }

    public static Post NewPost(User author)
    {
        return NewPost(author, "test post");
    }

    public static Post NewPost(User author, string text)
    {
        return NewPost(author, text, PictureId.New());
    }

    public static Post NewPost(User author, string text, PictureId picture)
    {
        return new Post(author.ID, picture, (PostText)text);
    }

    public static Comment CommentOn(Post post, User author, string text)
    {
        return post.CommentBy(author.ID, (CommentText)text);
    }

    private static User CreateUser(string nickname, string email, string password)
    {
        return new User((Nickname)nickname, (FullName)$"{nickname} user", Password.Create(password).Value, (Email)email, "my bio");
    }
}
