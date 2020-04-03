using System.Collections.Generic;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace InstaLike.Core.Domain
{
    public class FullName : ValueObject
    {
        public string Name { get; }
        public string Surname { get; }

        private FullName() { }

        private FullName(string name, string surname) : this()
        {
            Name = name;
            Surname = surname;
        }

        public static Result<FullName> Create(string name, string surname)
        {
            if (string.IsNullOrEmpty(name))
            {
                return Result.Failure<FullName>("You have to specify a proper name. An empty string is not allowed.");
            }

            if (string.IsNullOrEmpty(surname))
            {
                return Result.Failure<FullName>("You have to specify a proper surname. An empty string is not allowed.");
            }

            return Result.Success(new FullName(name, surname));
        }

        public static explicit operator FullName(string fullname)
        {
            fullname = Regex.Replace(fullname, @"\s+", " ");
            var splittedFullName = fullname.Split(new[] { ' ' }, 2);
            return Create(splittedFullName[0], splittedFullName[1]).Value;
        }

        public static implicit operator string(FullName fullname)
        {
            return fullname.ToString();
        }

        public override string ToString()
        {
            return $"{Name} {Surname}";
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name.ToUpperInvariant();
            yield return Surname.ToUpperInvariant();
        }
    }
}
