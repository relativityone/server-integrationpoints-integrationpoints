using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Security.Tests
{
	using NUnit.Framework;

	[TestFixture]
    public class DefaultEncryptionManagerTests
	{
		private DefaultEncryptionManager _defaultEncryptionManager;

		private string _decryptedText = "Test";

		private string _encryptedText = "mkb6vlTjbvQ=";

		[SetUp]
		public void Setup()
		{
			_defaultEncryptionManager= new DefaultEncryptionManager();
		}

		[Test]
		public void ItShouldEncryptText()
		{
			string result = this._defaultEncryptionManager.Encrypt(this._decryptedText);

			Assert.AreEqual(this._encryptedText,result);
		}

		[Test]
		public void ItShouldDecryptText()
		{
			string result = this._defaultEncryptionManager.Decrypt(this._encryptedText);

			Assert.AreEqual(this._decryptedText, result);
		}
	}
}
