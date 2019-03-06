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

        public User(Nickname nickname, string name, string surname, Password password, Email email, string biography)
            : this()
        {
            _nickname = nickname ?? throw new ArgumentNullException(nameof(nickname));
            _email = email ?? throw new ArgumentNullException(nameof(email));
            _password = password ?? throw new ArgumentNullException(nameof(password));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Surname = surname ?? throw new ArgumentNullException(nameof(surname));
            Biography = biography ?? throw new ArgumentNullException(nameof(biography));

            ProfilePicture = Picture.DefaultProfilePicture;
            RegistrationDate = DateTimeOffset.Now;
        }

        private readonly string _nickname;
        public virtual Nickname Nickname => (Nickname)_nickname;

        public virtual string Name { get; protected set; }
        public virtual string Surname { get; protected set; }

        private string _password;
        public virtual Password Password
        {
            get => (Password)_password;
            set => _password = value;
        }
        
        private readonly string _email;
        public virtual Email Email => (Email)_email;
        
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

        public virtual bool IsFollowing(User user)
        {
            return Followers.Any(f => f.Following == user);
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
                var follow = Following
                    .Where(f => f.Follower == this && f.Following == user)
                    .Single();
                _following.Remove(follow);
            }
        }

        public virtual List<Claim> Claims => new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, ID.ToString(), ClaimValueTypes.Integer32),
                new Claim(ClaimTypes.Name, Nickname, ClaimValueTypes.String),
                new Claim(ClaimTypes.GivenName, Name, ClaimValueTypes.String),
                new Claim(ClaimTypes.Surname, Surname, ClaimValueTypes.String),
                new Claim(ClaimTypes.Email, Email, ClaimValueTypes.Email)
            };
    }
}