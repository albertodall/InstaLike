using System;
using System.Globalization;
using InstaLike.Web.Extensions;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace InstaLike.Web.Shared
{
    [HtmlTargetElement("content-date", TagStructure = TagStructure.WithoutEndTag)]
    public sealed class ContentDateTagHelper : TagHelper
    {
        public DateTimeOffset Value { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "time";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("datetime", Value.ToString("O", CultureInfo.InvariantCulture));
            output.Attributes.SetAttribute("title", Value.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture));
            output.Attributes.SetAttribute("data-format", Value.IsShownAsARelativeDate() ? "relative" : "day");
            output.Content.SetContent(Value.AsPastDays());
        }
    }
}
