using System;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Posts;

/// <summary>
/// Represents an event raised when a comment is published.
/// </summary>
/// <param name="PostID">The identifier of the post that received the comment.</param>
/// <param name="CommentAuthorID">The identifier of the comment's author.</param>
/// <param name="PostAuthorID">The identifier of the post's author.</param>
public sealed record CommentPublishedEvent(PostId PostID, Guid CommentAuthorID, Guid PostAuthorID) : IDomainEvent;
