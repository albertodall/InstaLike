using System;
using System.Globalization;
using Humanizer;

namespace InstaLike.Web.Extensions
{
    internal static class DateTimeExtensions
    {
        public static string AsPastDays(this DateTime date)
        {
            var days = (DateTimeOffset.UtcNow.DateTime - date).TotalDays;
            if (days <= 15)
            {
                return date.Humanize(true, DateTimeOffset.UtcNow.DateTime, CultureInfo.GetCultureInfo("it-it"));
            }
            else
            {
                return date.ToString("dddd dd/MM/yyyy", CultureInfo.GetCultureInfo("it-it"));
            }
        }
    }
}
