using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace InstaLike.Web.Extensions
{
    internal static class FormFileExtensions
    {
        public async static Task<byte[]> ToByteArrayAsync(this IFormFile formFile)
        {
            if (formFile == null)
            {
                throw new ArgumentNullException(nameof(formFile));
            }

            byte[] result;
            using (var stream = new MemoryStream())
            {
                await formFile.CopyToAsync(stream);
                result = stream.ToArray();
            }

            return result;
        }

        public async static Task<Stream> ToStreamAsync(this IFormFile formFile)
        {
            if (formFile == null)
            {
                throw new ArgumentNullException(nameof(formFile));
            }

            var stream = new MemoryStream();
            await formFile.CopyToAsync(stream);
            stream.Position = 0;
            return stream;
        }
    }
}
