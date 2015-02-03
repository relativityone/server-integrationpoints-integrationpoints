namespace kCura.IntegrationPoints.LDAPProvider
{
	public interface IEncryptionManager
	{
		string Encrypt(string message);
		string Decrypt(string message);
	}
}
