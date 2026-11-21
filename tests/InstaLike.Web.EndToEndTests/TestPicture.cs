using System.IO;

namespace InstaLike.Web.EndToEndTests
{
    /// <summary>
    /// The picture the tests upload, embedded in the test assembly.
    /// </summary>
    internal static class TestPicture
    {
        public const string FileName = "grumpy_cat.jpg";
        public const string MediaType = "image/jpeg";

        public static byte[] Content { get; } = Load();

        /// <summary>
        /// Reads the picture from the test assembly's resources.
        /// </summary>
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
