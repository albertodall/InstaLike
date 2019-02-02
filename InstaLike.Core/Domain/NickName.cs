using System.Collections.Generic;
using CSharpFunctionalExtensions;

namespace InstaLike.Core.Domain
{
    public class NickName : ValueObject
    {
        public virtual string Value { get; }

        protected NickName()
        { }

        private NickName(string nickName)
        {
            Value = nickName;
        }

        public static Result<NickName> Create(string nickName)
        {
            var nick = (nickName ?? string.Empty).Trim();

            if (nick.Length == 0)
            {
                return Result.Fail<NickName>("Nickname should not be empty");
            }

            if (nick.Length > 20)
            {
                return Result.Fail<NickName>("Nickname is too long (max 20 chars)");
            }

            return Result.Ok(new NickName(nick));
        }

        public static implicit operator string(NickName nickName)
        {
            return nickName.Value;
        }

        public static explicit operator NickName(string nickName)
        {
            return Create(nickName).Value;
        }

        public override string ToString()
        {
            return Value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}