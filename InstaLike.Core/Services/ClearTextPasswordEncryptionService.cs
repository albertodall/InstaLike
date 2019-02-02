namespace InstaLike.Core.Services
{
    public class ClearTextPasswordEncryptionService : IPasswordEncryptionService
    {
        public string Encrypt(string password)
        {
            return password;
        }
    }
}
