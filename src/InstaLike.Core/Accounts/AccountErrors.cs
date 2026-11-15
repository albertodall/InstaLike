using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Accounts
{
    public static class AccountErrors
    {
        public static Error NotFound(AccountId accountID) =>
            Error.NotFound("Account.NotFound", $"Account {accountID} does not exist.");

        public static readonly Error InvalidCredentials =
            Error.Failure("Account.InvalidCredentials", "Username or password are not valid.");

        public static readonly Error WrongCurrentPassword =
            Error.Failure("CurrentPassword.Invalid", "The current password is not valid.");

        public static readonly Error UsernameEmpty =
            Error.Validation("Username.Empty", "Username should not be empty");

        public static readonly Error UsernameTooLong =
            Error.Validation("Username.TooLong", $"Username is too long (max {Username.UsernameMaxLength} chars)");

        public static readonly Error EmailInUse =
            Error.Conflict("Email.InUse", "This email is already used by another account.");

        public static readonly Error EmailEmpty =
            Error.Validation("Email.Empty", "Email should not be empty");

        public static readonly Error EmailInvalid =
            Error.Validation("Email.Invalid", "Email is invalid");

        public static readonly Error PasswordEmpty =
            Error.Validation("Password.Empty", "Password cannot be empty.");

        public static readonly Error PasswordTooShort =
            Error.Validation("Password.TooShort", $"Password is too short. Minimum allowed length is {Password.MinimumPasswordLength} characters. ");
    }
}
