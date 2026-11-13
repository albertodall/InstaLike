using System;
using InstaLike.Core.Accounts;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Membership
{
    /// <summary>
    /// Represents a member of the community and the public profile other members see.
    /// </summary>
    public class Member : AggregateRoot<MemberId>
    {
#pragma warning disable CS8618
        protected Member() { }
#pragma warning restore CS8618

        private Member(Nickname nickname, FullName fullName, string biography)
            : base(MemberId.New())
        {
            _nickname = nickname;
            _fullName = fullName;
            Biography = biography;
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

        public virtual PictureId? ProfilePicture { get; protected set; }
        public virtual string Biography { get; protected set; }
        public virtual DateTimeOffset RegistrationDate { get; protected set; }

        /// <summary>
        /// Registers a new member; the credentials are not kept by the member but travel in the
        /// <see cref="MemberRegisteredEvent"/> to the member's account.
        /// </summary>
        public static Member Register(Nickname nickname, FullName fullName, string biography, Username username, Password password, Email email)
        {
            var member = new Member(nickname, fullName, biography);
            member.RaiseDomainEvent(new MemberRegisteredEvent(member.ID, username, password, email));
            return member;
        }

        public virtual void SetProfilePicture(PictureId picture)
        {
            ProfilePicture = picture;
        }

        public virtual void RemoveProfilePicture()
        {
            ProfilePicture = null;
        }

        public virtual void ChangeNickname(Nickname nickname)
        {
            Nickname = nickname;
        }

        public virtual void ChangeFullName(FullName fullName)
        {
            FullName = fullName;
        }

        public virtual void UpdateBiography(string newBiography)
        {
            Biography = newBiography;
        }
    }
}
