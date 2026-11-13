using System;
using System.Diagnostics.CodeAnalysis;
using InstaLike.Core.SharedKernel;
using UUIDNext;

namespace InstaLike.Core.Notifications;

/// <summary>
/// Identifies a notification.
/// </summary>
/// <remarks>
/// New identifiers are generated in the application, in the order SQL Server sorts them, so they are known before the
/// notification is stored and do not fragment the clustered index.
/// </remarks>
public sealed record NotificationId(Guid Value) : ITypedId<NotificationId>
{
    public static NotificationId New() => new(Uuid.NewDatabaseFriendly(Database.SqlServer));

    public static NotificationId From(Guid value) => new(value);

    public static NotificationId Parse(string s, IFormatProvider? provider) => new(Guid.Parse(s, provider));

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out NotificationId result)
    {
        result = Guid.TryParse(s, provider, out var value) ? new NotificationId(value) : null;
        return result is not null;
    }

    public override string ToString() => Value.ToString();
}
