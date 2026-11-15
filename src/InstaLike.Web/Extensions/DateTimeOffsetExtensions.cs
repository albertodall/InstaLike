using System;
using System.Globalization;
using Humanizer;

namespace InstaLike.Web.Extensions
{
    public static class DateTimeOffsetExtensions
    {
        private const int RelativeDisplayDays = 15;

        public static string AsPastDays(this DateTimeOffset date)
        {
            return date.AsPastDays(DateTimeOffset.UtcNow);
        }

        public static string AsPastDays(this DateTimeOffset date, DateTimeOffset now)
        {
            if (date.IsShownAsARelativeDate(now))
            {
                return date.Humanize(now, CultureInfo.CurrentUICulture);
            }

            return date.ToString("dddd dd/MM/yyyy", CultureInfo.CurrentUICulture);
        }

        public static bool IsShownAsARelativeDate(this DateTimeOffset date)
        {
            return date.IsShownAsARelativeDate(DateTimeOffset.UtcNow);
        }

        public static bool IsShownAsARelativeDate(this DateTimeOffset date, DateTimeOffset now)
        {
            return (now - date).TotalDays <= RelativeDisplayDays;
        }
    }
}
