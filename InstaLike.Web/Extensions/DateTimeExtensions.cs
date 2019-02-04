using System;
using System.Globalization;
using Humanizer;

namespace InstaLike.Web.Extensions
{
    internal static class DateTimeExtensions
    {
        public static string AsDaysPassed(this DateTime data)
        {
            var days = (DateTimeOffset.UtcNow.DateTime - data).TotalDays;
            if (days <= 15)
            {
                return data.Humanize(true, DateTimeOffset.UtcNow.DateTime, CultureInfo.GetCultureInfo("it-it"));
            }
            else
            {
                return data.ToString("dddd dd/MM/yyyy", CultureInfo.GetCultureInfo("it-it"));
            }
        }
    }
}
