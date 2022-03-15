using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace kCura.IntegrationPoints.Security
{
	public class DefaultEncryptionManager : IEncryptionManager
	{
		//TODO: this does not belong here, we need to wait until we have a more permanent solution from the portland team
		private readonly string Salt =
			"w2hwalh15uj8k0qvahvdgab6q,57cb-m2.wrcim1n9u0rmr2o6dbmltkqz4,9o9l45od67nt-5p0j5ig3gnmqj.behtq.hvraq34m01u7rng85vatzjr5bcu1,584n6j60b1ukshawur9jlz4brcvflhbi6rl1ef628u6k6ix1p8uqk9435mu9ap5-9,-bv64-j1ru0s2ccu6uj5ryo,711deef550da-ks38ffa,-tv9";

		private readonly int KeyLength = 24;
		private readonly int InjectionLength = 16;

		//'<summary>
		//'Returns a two dimensional byte array.
		//'result[0] contains the key vector generated from the salt.
		//'result[1] contains the injection vector generated from the salt.
		//'</summary>
		//'<returns></returns>
		private byte[][] GenerateVectors()
		{
			byte[][] result = new byte[3][];

			byte[] saltBytes = new ASCIIEncoding().GetBytes(Salt);

			result[0] = new byte[KeyLength];
			result[1] = new byte[InjectionLength];

			Array.Copy(saltBytes, 0, result[0], 0, KeyLength);
			Array.Copy(saltBytes, KeyLength, result[1], 0, InjectionLength);

			return result;
		}

		public string Decrypt(string encryptedText)
		{
			if (string.IsNullOrWhiteSpace(encryptedText))
			{
				return string.Empty;
			}

			byte[][] vectors = GenerateVectors();
			//Do not change TripleDESCryptoServiceProvider as it is required for migration of old IntegrationPoints into SecretStore (when upgrading form old Relativity versions) 
			var crypto = new TripleDESCryptoServiceProvider();// NOSONAR
			ICryptoTransform decryptor = crypto.CreateDecryptor(vectors[0], vectors[1]);

			byte[] cipher = Convert.FromBase64String(encryptedText);
			MemoryStream memoryStream = new MemoryStream(cipher);
			CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

			byte[] decryptedText = new byte[cipher.Length + 1];
			int textLength = cryptoStream.Read(decryptedText, 0, decryptedText.Length);

			memoryStream.Close();
			cryptoStream.Close();

			return new ASCIIEncoding().GetString(decryptedText, 0, textLength);
		}

		public string Encrypt(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return string.Empty;
			}

			byte[][] vectors = GenerateVectors();
			//Do not change TripleDESCryptoServiceProvider as it is required for migration of old IntegrationPoints into SecretStore (when upgrading form old Relativity versions) 
			var crypto = new TripleDESCryptoServiceProvider();// NOSONAR
			ICryptoTransform encryptor = crypto.CreateEncryptor(vectors[0], vectors[1]);
			MemoryStream memoryStream = new MemoryStream();
			CryptoStream CryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);

			byte[] encodedText = new ASCIIEncoding().GetBytes(text);
			CryptoStream.Write(encodedText, 0, encodedText.Length);
			CryptoStream.FlushFinalBlock();

			CryptoStream.Close();
			memoryStream.Close();

			return Convert.ToBase64String(memoryStream.ToArray());
		}
	}
}