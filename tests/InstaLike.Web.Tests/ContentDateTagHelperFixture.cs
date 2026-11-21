using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Web.Shared;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Xunit;

namespace InstaLike.Web.Tests
{
    public sealed class ContentDateTagHelperFixture
    {
        public ContentDateTagHelperFixture()
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
        }

        [Fact]
        public void A_Recent_Date_Is_A_Relative_Time_Element()
        {
            var aMinuteAgo = DateTimeOffset.UtcNow.AddMinutes(-1);

            var output = Render(aMinuteAgo);

            using (new AssertionScope())
            {
                output.TagName.Should().Be("time");
                output.TagMode.Should().Be(TagMode.StartTagAndEndTag);
                output.Attributes["datetime"].Value.Should().Be(aMinuteAgo.ToString("O", CultureInfo.InvariantCulture));
                output.Attributes["title"].Value.Should().Be(aMinuteAgo.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture));
                output.Attributes["data-format"].Value.Should().Be("relative");
                output.Content.GetContent().Should().Be("a minute ago");
            }
        }

        [Fact]
        public void An_Old_Date_Is_A_Day_Time_Element()
        {
            var oldDate = new DateTimeOffset(2020, 3, 4, 23, 16, 14, TimeSpan.FromHours(1));

            var output = Render(oldDate);

            using (new AssertionScope())
            {
                output.Attributes["datetime"].Value.Should().Be("2020-03-04T23:16:14.0000000+01:00");
                output.Attributes["title"].Value.Should().Be("04/03/2020 23:16");
                output.Attributes["data-format"].Value.Should().Be("day");
                output.Content.GetContent().Should().Be("Wednesday 04/03/2020");
            }
        }

        /// <summary>
        /// Renders the content-date tag helper for the date and returns its output.
        /// </summary>
        private static TagHelperOutput Render(DateTimeOffset value)
        {
            var context = new TagHelperContext([], new Dictionary<object, object>(), "test");
            var output = new TagHelperOutput(
                "content-date",
                [],
                (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

            new ContentDateTagHelper()
            {
                Value = value
            }.Process(context, output);

            return output;
        }
    }
}
