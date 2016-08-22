namespace kCura.IntegrationPoints.Security
{
    public interface IEncryptionManager
    {
        string Encrypt(string message);
        string Decrypt(string message);
    }
}
