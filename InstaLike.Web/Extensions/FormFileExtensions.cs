using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace InstaLike.Web.Extensions
{
    internal static class FormFileExtensions
    {
        public static Task<byte[]> ToByteArrayAsync(this IFormFile formFile)
        {
            return formFile.ToByteArrayInternalAsync();
        }

        private static async Task<byte[]> ToByteArrayInternalAsync(this IFormFile formFile)
        {
            byte[] result;
            await using (var stream = new MemoryStream())
            {
                await formFile.CopyToAsync(stream);
                result = stream.ToArray();
            }

            return result;
        }

        public static Task<Stream> ToStreamAsync(this IFormFile formFile)
        {
            return formFile.ToStreamInternalAsync();
        }

        private static async Task<Stream> ToStreamInternalAsync(this IFormFile formFile)
        {
            var stream = new MemoryStream();
            await formFile.CopyToAsync(stream);
            stream.Position = 0;
            return stream;
        }
    }
}
