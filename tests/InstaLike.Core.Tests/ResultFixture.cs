using System;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.SharedKernel;
using Xunit;

namespace InstaLike.Core.Tests
{
    public sealed class ResultFixture
    {
        private static readonly Error FirstError = Error.Validation("First.Invalid", "First is invalid.");
        private static readonly Error SecondError = Error.Conflict("Second.Taken", "Second is taken.");
        private static readonly Error ThirdError = Error.NotFound("Third.Missing", "Third is missing.");

        [Fact]
        public void Combining_Successful_Results_Should_Succeed()
        {
            Result.Combine(Result.Ok(), Result.Ok("value")).IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void Combining_One_Failure_Should_Keep_Its_Error()
        {
            var sut = Result.Combine(Result.Ok(), Result.Failure<string>(FirstError));

            using (new AssertionScope())
            {
                sut.IsFailure.Should().BeTrue();
                sut.Error.Should().Be(FirstError);
            }
        }

        [Fact]
        public void Combining_Several_Failures_Should_Group_Their_Errors()
        {
            var sut = Result.Combine(Result.Failure(FirstError), Result.Ok(), Result.Failure<string>(SecondError));

            using (new AssertionScope())
            {
                sut.IsFailure.Should().BeTrue();
                sut.Error.Should().BeOfType<MultipleErrors>()
                    .Which.Errors.Should().Equal(FirstError, SecondError);
                sut.Error.Description.Should().Be("First is invalid. Second is taken.");
            }
        }

        [Fact]
        public void Combining_Grouped_Errors_Should_Keep_Them_Flat()
        {
            var grouped = Result.Combine(Result.Failure(FirstError), Result.Failure(SecondError));

            var sut = Result.Combine(grouped, Result.Failure(ThirdError));

            sut.Error.Should().BeOfType<MultipleErrors>()
                .Which.Errors.Should().Equal(FirstError, SecondError, ThirdError);
        }

        [Fact]
        public void Combining_Errors_Of_The_Same_Type_Should_Keep_That_Type()
        {
            var otherConflict = Error.Conflict("Other.Taken", "Other is taken.");

            var sut = Result.Combine(Result.Failure(SecondError), Result.Failure(otherConflict));

            sut.Error.Type.Should().Be(ErrorType.Conflict);
        }

        [Fact]
        public void Combining_Errors_Of_Different_Types_Should_Be_A_Failure()
        {
            var sut = Result.Combine(Result.Failure(FirstError), Result.Failure(SecondError));

            sut.Error.Type.Should().Be(ErrorType.Failure);
        }

        [Fact]
        public void A_Failure_Should_Not_Accept_The_None_Error()
        {
            var createFailure = () => Result.Failure(Error.None);

            createFailure.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void A_Failed_Result_Should_Have_No_Value()
        {
            var sut = Result.Failure<string>(FirstError);

            sut.Invoking(r => r.Value).Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void A_Failed_Result_Should_Describe_Its_Error()
        {
            var sut = Result.Failure<string>(FirstError);

            sut.ToString().Should().Contain(FirstError.Code);
        }

        [Fact]
        public void A_Default_Result_Should_Be_A_Failure()
        {
            using (new AssertionScope())
            {
                default(Result).IsFailure.Should().BeTrue();
                default(Result<string>).IsFailure.Should().BeTrue();
            }
        }

        [Fact]
        public void FailureIf_Should_Fail_Only_When_The_Condition_Holds()
        {
            using (new AssertionScope())
            {
                Result.FailureIf(true, FirstError).Error.Should().Be(FirstError);
                Result.FailureIf(false, FirstError).IsSuccess.Should().BeTrue();
            }
        }
    }
}
