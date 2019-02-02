using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;

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

        public User(string nickName, string firstName, string lastName, Email email)
        {
            _nickName = nickName ?? throw new ArgumentNullException(nameof(nickName));
            _email = email ?? throw new ArgumentNullException(nameof(email)) ;
            FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
            LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
        }

        private string _nickName;
        public virtual NickName NickName
        {
            get => (NickName)_nickName;
            set => _nickName = value;
        }

        public virtual string FirstName { get; protected set; }
        public virtual string LastName { get; protected set; }

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

        public virtual Result ChangePassword(string password)
        {
            var passwordResult = Password.Create(password);
            if (passwordResult.IsFailure)
            {
                return Result.Fail(passwordResult.Error);
            }
            return Result.Ok();
        }
    }
}