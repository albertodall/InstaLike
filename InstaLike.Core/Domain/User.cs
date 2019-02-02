using System;
using CSharpFunctionalExtensions;
using InstaLike.Core.Services;

namespace InstaLike.Core.Domain
{
    public class User : EntityBase<int>
    {
        protected User()
        { }

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

        public virtual string Bio { get; protected set; }

        public virtual DateTimeOffset RegistrationDate { get; protected set; }

        public virtual Result SetPassword(string password, IPasswordEncryptionService encryptor)
        {
            var passwordResult = Password.Create(password);
            if (passwordResult.IsFailure)
            {
                return Result.Fail(passwordResult.Error);
            }
            _password = encryptor.Encrypt(passwordResult.Value);
            return Result.Ok();
        }
    }
}
