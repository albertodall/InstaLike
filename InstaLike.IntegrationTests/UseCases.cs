using System;
using FluentAssertions;
using InstaLike.Core.Commands;
using InstaLike.Web.CommandHandlers;
using Serilog;
using Xunit;

namespace InstaLike.IntegrationTests
{
    public class UseCases : DatabaseFixture
    {
        [Fact]
        public async void Should_Register_User()
        {
            var userRegistrationCommand = new RegisterUserCommand(
                "testuser", 
                "test", 
                "user", 
                "password", 
                "testuser@acme.com",
                "test user bio",
                Convert.FromBase64String(Test_Picture_Base64));

            using (var session = OpenSession())
            {
                var handler = new RegisterUserCommandHandler(session, Log.Logger);
                var result = await handler.Handle(userRegistrationCommand, default);
                result.IsSuccess.Should().BeTrue();
            }
        }
    }
}