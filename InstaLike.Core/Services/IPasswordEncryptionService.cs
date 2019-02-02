namespace InstaLike.Core.Services
{
    public interface IPasswordEncryptionService
    {
        string Encrypt(string password);
    }
}
