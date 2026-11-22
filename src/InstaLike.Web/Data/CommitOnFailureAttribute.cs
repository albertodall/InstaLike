using System;

namespace InstaLike.Web.Data
{
    /// <summary>
    /// Commits the changes and the domain events of a command also when it returns a failed result,
    /// for the commands whose failed attempts change the state.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class CommitOnFailureAttribute : Attribute
    {
    }
}
