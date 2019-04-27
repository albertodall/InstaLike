using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
            _followed  = new List<Follow>();
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

        public virtual Result Follow(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (!IsFollowing(user))
            {
                _followed.Add(new Follow(this, user));
                user.AddFollower(this);
                return Result.Ok();
            }

            return Result.Fail($"User '{user.Nickname}' is already followed by '{Nickname}'.");
        }

        protected internal virtual void AddFollower(User follower)
        {
            _followers.Add(new Follow(follower, this));
        }

        public virtual Result Unfollow(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (IsFollowing(user))
            {
                var follow = _followed.Single(f => f.Followed == user);
                _followed.Remove(follow);
                user.RemoveFollower(this);
                return Result.Ok();
            }

            return Result.Fail($"User '{Nickname}' is not following '{user.Nickname}'.");
        }

        protected internal virtual void RemoveFollower(User follower)
        {
            var follow = _followers.Single(f => f.Follower == follower);
            _followers.Remove(follow);
        }

        public virtual bool Likes(Post post)
        {
            if (post == null)
            {
                throw new ArgumentNullException(nameof(post));
            }

            return post.LikesTo(this);
        }

        public virtual Result PutLikeTo(Post post)
        {
            if (post == null)
            {
                throw new ArgumentNullException(nameof(post));
            }

            if (post.Author == this)
            {
                return Result.Fail("You cannot put a 'Like' on your own posts.");
            }

            if (Likes(post))
            {
                return Result.Fail("This user already 'Liked' this post.");
            }

            post.PutLikeBy(this);
            return Result.Ok();
        }

        public virtual Result RemoveLikeFrom(Post post)
        {
            if (post == null)
            {
                throw new ArgumentNullException(nameof(post));
            }

            if (Likes(post))
            {
                post.RemoveLikeBy(this);
                return Result.Ok();
            }

            return Result.Fail($"User '{Nickname}' did not put any 'Like' on this post.");
        }

        public virtual List<Claim> Claims => new List<Claim>()
        {
            new Claim(ClaimTypes.NameIdentifier, ID.ToString(), ClaimValueTypes.Integer32),
            new Claim(ClaimTypes.Name, Nickname, ClaimValueTypes.String),
            new Claim(ClaimTypes.GivenName, FullName.Name, ClaimValueTypes.String),
            new Claim(ClaimTypes.Surname, FullName.Surname, ClaimValueTypes.String),
            new Claim(ClaimTypes.Email, Email, ClaimValueTypes.Email)
        };
    }
}