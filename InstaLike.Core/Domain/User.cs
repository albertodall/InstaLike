using System;
using System.Collections.Generic;
using System.Linq;

namespace InstaLike.Core.Domain
{
    public class User : EntityBase<int>
    {
        private readonly IList<User> _followers;
        private readonly IList<User> _following;

        protected User()
        {
            _followers = new List<User>();
            _following = new List<User>();
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
        public virtual IReadOnlyList<User> Followers => _followers.ToList();
        public virtual IReadOnlyList<User> Following => _following.ToList();

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
    }
}