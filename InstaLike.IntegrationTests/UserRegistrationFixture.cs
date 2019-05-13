using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Commands;
using InstaLike.Core.Domain;
using InstaLike.Web.CommandHandlers;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace InstaLike.IntegrationTests
{
    public class UserRegistrationFixture : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _testFixture;
        private readonly ITestOutputHelper _output;

        public UserRegistrationFixture(DatabaseFixture fixture, ITestOutputHelper output)
        {
            _testFixture = fixture;
            _output = output;
        }

        [Fact]
        public async Task Should_Register_User()
        {
            Result<int> userRegistrationResult;

            var userRegistrationCommand = new RegisterUserCommand(
                "registereduser",
                "registered",
                "user",
                "password",
                "registereduser@acme.com",
                "registered user bio",
                Convert.FromBase64String(_testFixture.GetTestPictureBase64()));

            using (var session = _testFixture.OpenSession(_output))
            {
                var sut = new RegisterUserCommandHandler(session, Log.Logger);
                userRegistrationResult = await sut.Handle(userRegistrationCommand, default);
            }

            using (new AssertionScope())
            {
                userRegistrationResult.IsSuccess.Should().BeTrue();
                userRegistrationResult.Value.Should().BeGreaterThan(0);
                using (var session = _testFixture.OpenSession(_output))
                {
                    (await session.QueryOver<User>().RowCountAsync()).Should().Be(1);
                }
            }
        }
    }
}
