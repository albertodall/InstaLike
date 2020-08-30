using System;
using System.Globalization;
using Humanizer;

namespace InstaLike.Web.Extensions
{
    public static class DateTimeExtensions
    {
        public static string AsPastDays(this DateTime date)
        {
            var days = (DateTimeOffset.UtcNow.DateTime - date).TotalDays;
            if (days <= 15)
            {
                return date.Humanize(true, DateTimeOffset.UtcNow.DateTime, CultureInfo.CurrentUICulture);
            }

            return date.ToString("dddd dd/MM/yyyy", CultureInfo.CurrentUICulture);
        }
    }
}