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

        protected User()
        {
            _followers = new List<Follow>();
            _followed = new List<Follow>();
        }

        public User(Nickname nickname, FullName fullName, Password password, Email email, string biography)
            : this()
        {
            _nickname = nickname ?? throw new ArgumentNullException(nameof(nickname));
            _email = email ?? throw new ArgumentNullException(nameof(email));
            _password = password ?? throw new ArgumentNullException(nameof(password));
            _fullName = fullName ?? throw new ArgumentNullException(nameof(fullName));
            Biography = biography ?? throw new ArgumentNullException(nameof(biography));

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
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return _followed.Any(f => f.Followed == user);
        }

        public virtual bool IsFollowedBy(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return _followers.Any(f => f.Follower == user);
        }

        public virtual Result Follow(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Result.Ok()
                .Ensure(() => !IsFollowing(user), $"User [{user.Nickname}] is already followed by user [{Nickname}].")
                .OnSuccess(() =>
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
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            Maybe<Follow> followed = _followed.SingleOrDefault(f => f.Followed == user);

            return followed
                .ToResult($"User [{Nickname}] is not following user [{user.Nickname}].")
                .OnSuccess(follow =>
                {
                    _followed.Remove(follow);
                    user.RemoveFollow(follow);
                });
        }

        protected virtual void RemoveFollow(Follow follow)
        {
            _followers.Remove(follow);
        }

        public virtual Result PutLikeTo(Post post)
        {
            if (post == null)
            {
                throw new ArgumentNullException(nameof(post));
            }

            return Result.Ok()
                .Ensure(() => post.Author != this, "You cannot put a 'Like' on your own posts.")
                .Ensure(() => !post.LikesTo(this), $"User [{Nickname}] already 'Liked' this post.")
                .OnSuccess(() => post.PutLikeBy(this));
        }

        public virtual Result RemoveLikeFrom(Post post)
        {
            if (post == null)
            {
                throw new ArgumentNullException(nameof(post));
            }

            return Result.Ok()
                .Ensure(() => post.LikesTo(this), $"User [{Nickname}] did not put any 'Like' on this post.")
                .OnSuccess(() => post.RemoveLikeBy(this));
        }
    }
}