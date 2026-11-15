using System;
using System.Globalization;
using FluentAssertions;
using InstaLike.Web.Extensions;
using Xunit;

namespace InstaLike.IntegrationTests
{
    public sealed class RelativeDateFixture
    {
        private static readonly DateTimeOffset Now = new(2026, 11, 15, 10, 0, 0, TimeSpan.Zero);
        private static readonly TimeSpan ItalianWinterTime = TimeSpan.FromHours(1);

        public RelativeDateFixture()
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
        }

        [Fact]
        public void A_Date_Stored_With_An_Offset_Is_Compared_To_The_Same_Instant()
        {
            var aMinuteAgo = Now.AddMinutes(-1).ToOffset(ItalianWinterTime);

            aMinuteAgo.AsPastDays(Now).Should().Be("a minute ago");
        }

        [Fact]
        public void A_Date_From_The_Local_Clock_Is_In_The_Past()
        {
            var aMinuteAgo = DateTimeOffset.Now.AddMinutes(-1);

            aMinuteAgo.AsPastDays().Should().Be("a minute ago");
        }

        [Fact]
        public void A_Date_Within_Fifteen_Days_Is_Relative()
        {
            var fifteenDaysAgo = Now.AddDays(-15).ToOffset(ItalianWinterTime);

            fifteenDaysAgo.AsPastDays(Now).Should().Be("15 days ago");
        }

        [Fact]
        public void A_Date_Older_Than_Fifteen_Days_Shows_The_Day()
        {
            var octoberFirst = new DateTimeOffset(2026, 10, 1, 18, 30, 0, TimeSpan.FromHours(2));

            octoberFirst.AsPastDays(Now).Should().Be("Thursday 01/10/2026");
        }

        [Fact]
        public void Only_Dates_Within_Fifteen_Days_Are_Shown_As_Relative_Dates()
        {
            Now.AddDays(-15).IsShownAsARelativeDate(Now).Should().BeTrue();
            Now.AddDays(-16).IsShownAsARelativeDate(Now).Should().BeFalse();
        }
    }
}
