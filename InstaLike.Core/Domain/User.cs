using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace InstaLike.Core.Domain
{
    public class User : EntityBase<int>
    {
        private readonly IList<Follow> _followers;
        private readonly IList<Follow> _following;

        protected User()
        {
            _followers = new List<Follow>();
            _following = new List<Follow>();
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
            set => _nickname = value;
        }

        private string _fullName;
        public virtual FullName FullName
        {
            get => (FullName)_fullName;
            set => _fullName = value;
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
        public virtual IReadOnlyList<Follow> Following => _following.ToList();

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

            return _following.Any(f => f.Following == user);
        }

        public virtual void Follow(User user)
        {
            if (!IsFollowing(user))
            {
                _followers.Add(new Follow(this, user));
            }
        }

        public virtual void Unfollow(User user)
        {
            if (IsFollowing(user))
            {
                var follow = _following
                    .Single(f => f.Follower == this && f.Following == user);
                _following.Remove(follow);
            }
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