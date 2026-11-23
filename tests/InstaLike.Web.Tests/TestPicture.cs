using System.IO;

namespace InstaLike.Web.Tests
{
    /// <summary>
    /// A real 700 × 933 JPEG picture, embedded in the test assembly.
    /// </summary>
    internal static class TestPicture
    {
        private const string FileName = "grumpy_cat.jpg";

        public static byte[] Content { get; } = Load();

        public static MemoryStream OpenRead() => new(Content, false);

        private static byte[] Load()
        {
            using var resource = typeof(TestPicture).Assembly.GetManifestResourceStream(FileName)
                ?? throw new FileNotFoundException("The test picture is not embedded in the test assembly.", FileName);
            using var content = new MemoryStream();
            resource.CopyTo(content);

            return content.ToArray();
        }
    }
}
