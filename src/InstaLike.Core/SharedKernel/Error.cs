namespace InstaLike.Core.SharedKernel
{
    /// <summary>
    /// Describes why an operation failed, with a code that identifies the error and a message for the user.
    /// </summary>
    public record Error
    {
        public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);

        public string Code { get; }
        public string Description { get; }
        public ErrorType Type { get; }

        protected Error(string code, string description, ErrorType type)
        {
            Code = code;
            Description = description;
            Type = type;
        }

        public static Error Failure(string code, string description) =>
            new(code, description, ErrorType.Failure);

        public static Error Validation(string code, string description) =>
            new(code, description, ErrorType.Validation);

        public static Error NotFound(string code, string description) =>
            new(code, description, ErrorType.NotFound);

        public static Error Conflict(string code, string description) =>
            new(code, description, ErrorType.Conflict);
    }

    public enum ErrorType
    {
        None,
        Failure,
        Validation,
        NotFound,
        Conflict
    }
}
