using System;
using System.Text.RegularExpressions;

namespace InstaLike.Web.Extensions
{
    internal static class StringExtensions
    {
        private const string GuidRegex =
            @"([0-9a-fA-F]{8})\-([0-9a-fA-F]{4})\-([0-9a-fA-F]{4})\-([0-9a-fA-F]{4})\-([0-9a-fA-F]{12})";

        private static bool ContainsGuid(this string stringValue) => Regex.IsMatch(stringValue, GuidRegex);

        public static string ExtractGuid(this string stringValue)
        {
            if (!stringValue.ContainsGuid())
            {
                throw new InvalidOperationException($"Value '{stringValue}' does not contain a Guid value.");
            } 
            
            return Regex.Match(stringValue, GuidRegex).Value;
        }
    }
}