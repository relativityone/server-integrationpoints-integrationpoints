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

		private string decryptedText = "Test";

		private string encryptedText = "mkb6vlTjbvQ=";

		[SetUp]
		public void Setup()
		{
			_defaultEncryptionManager= new DefaultEncryptionManager();
		}

		[Test]
		public void ItShouldEncryptText()
		{
			var result = this._defaultEncryptionManager.Encrypt(this.decryptedText);

			Assert.AreEqual(this.encryptedText,result);
		}

		[Test]
		public void ItShouldDecryptText()
		{
			var result = this._defaultEncryptionManager.Decrypt(this.encryptedText);

			Assert.AreEqual(this.decryptedText, result);
		}
	}
}
