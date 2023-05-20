using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;

namespace InstaLike.Core.Domain
{
    public class User : EntityBase<int>
    {
        private readonly IList<Follow> _followers;
        private readonly IList<Follow> _followed;

#pragma warning disable CS8618
        protected User()
        {
            _followers = new List<Follow>();
            _followed = new List<Follow>();
        }
#pragma warning restore CS8618

        public User(Nickname nickname, FullName fullName, Password password, Email email, string biography) 
            : this()
        {
            _nickname = nickname;
            _email = email;
            _password = password;
            _fullName = fullName;
            Biography = biography;

            ProfilePicture = Picture.DefaultProfilePicture;
            RegistrationDate = DateTimeOffset.Now;
        }

        private string _nickname;
        public virtual Nickname Nickname
        {
            get => (Nickname)_nickname;
            protected set => _nickname = value;
        }

        private string _fullName;
        public virtual FullName FullName
        {
            get => (FullName)_fullName;
            protected set => _fullName = value;
        }

        private string _password;
        public virtual Password Password
        {
            get => (Password)_password;
            set => _password = value;
        }
        
        private string _email;
        public virtual Email Email
        {
            get => (Email)_email;
            set => _email = value;
        }
        
        public virtual Picture ProfilePicture { get; protected set; }
        public virtual string Biography { get; protected set; }
        public virtual DateTimeOffset RegistrationDate { get; protected set; }
        public virtual IReadOnlyList<Follow> Followers => _followers.ToList();
        public virtual IReadOnlyList<Follow> Followed => _followed.ToList();

        public virtual void ChangePassword(Password password)
        {
            Password = password;
        }

        public virtual void SetProfilePicture(Picture picture)
        {
            ProfilePicture = picture;
        }

        public virtual void SetDefaultProfilePicture()
        {
            ProfilePicture = Picture.DefaultProfilePicture;
        }

        public virtual void ChangeNickname(Nickname nickname)
        {
            Nickname = nickname;
        }

        public virtual void ChangeFullName(FullName fullName)
        {
            FullName = fullName;
        }

        public virtual void ChangeEmailAddress(Email email)
        {
            Email = email;
        }

        public virtual void UpdateBiography(string newBiography)
        {
            Biography = newBiography;
        }

        public virtual bool IsFollowing(User user)
        {
            return _followed.Any(f => f.Followed == user);
        }

        public virtual bool IsFollowedBy(User user)
        {
            return _followers.Any(f => f.Follower == user);
        }

        public virtual Result Follow(User user)
        {
            return Result.Success()
                .Ensure(() => !IsFollowing(user), $"User [{user.Nickname}] is already followed by user [{Nickname}].")
                .Tap(() =>
                {
                    var follow = new Follow(this, user);
                    _followed.Add(follow);
                    user.AddFollow(follow);
                });
        }

        protected virtual void AddFollow(Follow follow)
        {
            _followers.Add(follow);
        }

        public virtual Result Unfollow(User user)
        {
            Follow? followed = _followed.SingleOrDefault(f => f.Followed == user);

            if (followed is null)
            {
                return Result.Failure($"User [{Nickname}] is not following user [{user.Nickname}].");
            }

            _followed.Remove(followed);
            user.RemoveFollow(followed);

            return Result.Success();
        }

        protected virtual void RemoveFollow(Follow follow)
        {
            _followers.Remove(follow);
        }

        public virtual Result PutLikeTo(Post post)
        {
            return Result.Success()
                .Ensure(() => post.Author != this, "You cannot put a 'Like' on your own posts.")
                .Ensure(() => !post.LikesTo(this), $"User [{Nickname}] already 'Liked' this post.")
                .Tap(() => post.PutLikeBy(this));
        }

        public virtual Result RemoveLikeFrom(Post post)
        {
            return Result.Success()
                .Ensure(() => post.LikesTo(this), $"User [{Nickname}] did not put any 'Like' on this post.")
                .Tap(() => post.RemoveLikeBy(this));
        }
    }
}