﻿using CSharpFunctionalExtensions;
using MediatR;

namespace InstaLike.Core.Commands
{
    public sealed class RegisterUserCommand : IRequest<Result<int>>
    {
        public string Nickname { get; }
        public string Name { get; }
        public string Surname { get; }
        public string Password { get; }
        public string Email { get; }
        public string Biography { get; }
        public byte[] ProfilePicture { get; }

        public RegisterUserCommand(string nickname, string name, string surname, string password, string email, string biography, byte[] profilePicture)
        {
            Nickname = nickname;
            Name = name;
            Surname = surname;
            Password = password;
            Email = email;
            Biography = biography;
            ProfilePicture = profilePicture;
        }
    }
}
