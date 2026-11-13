using System;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Posts;

/// <summary>
/// Represents an event raised when a post is liked.
/// </summary>
/// <param name="PostID">The identifier of the liked post.</param>
/// <param name="LikerID">The identifier of the member who liked the post.</param>
/// <param name="PostAuthorID">The identifier of the post's author.</param>
public sealed record PostLikedEvent(PostId PostID, Guid LikerID, Guid PostAuthorID) : IDomainEvent;
