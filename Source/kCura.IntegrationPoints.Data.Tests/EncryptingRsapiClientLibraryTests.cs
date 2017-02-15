using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.SecretStore;
using kCura.Relativity.Client.DTOs;
using NSubstitute;
using NUnit.Framework;
using Relativity.SecretCatalog;

namespace kCura.IntegrationPoints.Data.Tests
{
	[TestFixture]
	public class EncryptingRsapiClientLibraryTests : TestBase
	{
		private IGenericLibrary<IntegrationPoint> _library;
		private ISecretCatalog _secretCatalog;
		private ISecretManager _secretManager;

		private EncryptingRsapiClientLibrary _encryptingRsapiClientLibrary;

		public override void SetUp()
		{
			_library = Substitute.For<IGenericLibrary<IntegrationPoint>>();
			_secretCatalog = Substitute.For<ISecretCatalog>();
			_secretManager = Substitute.For<ISecretManager>();

			_encryptingRsapiClientLibrary = new EncryptingRsapiClientLibrary(_library, _secretCatalog, _secretManager);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void ItShouldEncryptWhileCreating(bool testWithEnumerable)
		{
			var expectedSecuredConfiguration = "{secured: 'config'}";
			var rdo = new IntegrationPoint
			{
				SecuredConfiguration = expectedSecuredConfiguration
			};

			var expectedDictionary = new Dictionary<string, string>();
			var expectedSecretRef = new SecretRef
			{
				SecretID = "456850",
				TenantID = "858508"
			};

			_secretManager.CreateSecretData(rdo).Returns(expectedDictionary);
			_secretManager.GenerateIdentifier().Returns(expectedSecretRef);

			if (testWithEnumerable)
			{
				_encryptingRsapiClientLibrary.Create(new[] {rdo});
				_library.Received(1).Create(Arg.Is<IEnumerable<IntegrationPoint>>(x => x.First().SecuredConfiguration == expectedSecuredConfiguration));
			}
			else
			{
				_encryptingRsapiClientLibrary.Create(rdo);
				_library.Received(1).Create(Arg.Is<IntegrationPoint>(x => x.SecuredConfiguration == expectedSecuredConfiguration));
			}

			_secretManager.Received(1).CreateSecretData(rdo);
			_secretManager.Received(1).GenerateIdentifier();
			_secretCatalog.Received(1).WriteSecret(expectedSecretRef, expectedDictionary);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void ItShouldDecryptWhileReading(bool testWithEnumerable)
		{
			var integrationPointArtifactId = 605147;
			var rdo = new IntegrationPoint
			{
				SecuredConfiguration = "secret"
			};

			_library.Read(integrationPointArtifactId).Returns(rdo);
			_library.Read(Arg.Is<IEnumerable<int>>(x => x.First() == integrationPointArtifactId)).Returns(new[] {rdo}.ToList());

			var expectedSecuredConfiguration = "config_885587";
			var expectedDictionary = new Dictionary<string, string>();
			var expectedSecretRef = new SecretRef
			{
				SecretID = "368463",
				TenantID = "319131"
			};

			_secretManager.RetrieveIdentifier(rdo).Returns(expectedSecretRef);
			_secretCatalog.GetSecret(expectedSecretRef).Returns(expectedDictionary);
			_secretManager.RetrieveValue(expectedDictionary).Returns(expectedSecuredConfiguration);

			IntegrationPoint actualRdo;

			if (testWithEnumerable)
			{
				actualRdo = _encryptingRsapiClientLibrary.Read(new[] {integrationPointArtifactId}).First();
				_library.Received(1).Read(Arg.Is<IEnumerable<int>>(x => x.First() == integrationPointArtifactId));
			}
			else
			{
				actualRdo = _encryptingRsapiClientLibrary.Read(integrationPointArtifactId);
				_library.Received(1).Read(integrationPointArtifactId);
			}

			Assert.That(actualRdo.SecuredConfiguration, Is.EqualTo(expectedSecuredConfiguration));

			_secretManager.Received(1).RetrieveIdentifier(rdo);
			_secretCatalog.Received(1).GetSecret(expectedSecretRef);
			_secretManager.Received(1).RetrieveValue(expectedDictionary);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void ItShouldEncryptWhileUpdating(bool testWithEnumerable)
		{
			var expectedSecuredConfiguration = "{secured: 'config'}";
			var rdo = new IntegrationPoint
			{
				ArtifactId = 801515,
				SecuredConfiguration = expectedSecuredConfiguration
			};
			var existingRdo = new IntegrationPoint();

			var expectedDictionary = new Dictionary<string, string>();
			var expectedSecretRef = new SecretRef
			{
				SecretID = "414710",
				TenantID = "200218"
			};

			_secretManager.CreateSecretData(rdo).Returns(expectedDictionary);
			_library.Read(rdo.ArtifactId).Returns(existingRdo);
			_secretManager.RetrieveIdentifier(existingRdo).Returns(expectedSecretRef);

			if (testWithEnumerable)
			{
				_encryptingRsapiClientLibrary.Update(new[] { rdo });
				_library.Received(1).Update(Arg.Is<IEnumerable<IntegrationPoint>>(x => x.First().SecuredConfiguration == expectedSecuredConfiguration));
			}
			else
			{
				_encryptingRsapiClientLibrary.Update(rdo);
				_library.Received(1).Update(Arg.Is<IntegrationPoint>(x => x.SecuredConfiguration == expectedSecuredConfiguration));
			}

			_secretManager.Received(1).CreateSecretData(rdo);
			_library.Received(1).Read(rdo.ArtifactId);
			_secretManager.Received(1).RetrieveIdentifier(existingRdo);
			_secretCatalog.Received(1).WriteSecret(expectedSecretRef, expectedDictionary);
		}

		[Test]
		public void ItShouldDecryptWhileQuerying()
		{
			var query = new Query<RDO>();
			var rdo = new IntegrationPoint
			{
				SecuredConfiguration = "secret"
			};

			_library.Query(query, 0).Returns(new[] {rdo}.ToList());

			var expectedSecuredConfiguration = "config_293570";
			var expectedDictionary = new Dictionary<string, string>();
			var expectedSecretRef = new SecretRef
			{
				SecretID = "249695",
				TenantID = "103987"
			};

			_secretManager.RetrieveIdentifier(rdo).Returns(expectedSecretRef);
			_secretCatalog.GetSecret(expectedSecretRef).Returns(expectedDictionary);
			_secretManager.RetrieveValue(expectedDictionary).Returns(expectedSecuredConfiguration);

			var actualRdos = _encryptingRsapiClientLibrary.Query(query);
			_library.Received(1).Query(query, 0);

			Assert.That(actualRdos[0].SecuredConfiguration, Is.EqualTo(expectedSecuredConfiguration));

			_secretManager.Received(1).RetrieveIdentifier(rdo);
			_secretCatalog.Received(1).GetSecret(expectedSecretRef);
			_secretManager.Received(1).RetrieveValue(expectedDictionary);
		}

		[Test(Description = "We do not delete secret here, as PreDeleteEventHandler is triggered when using Delete method from RSAPI")]
		public void ItShouldNotDeleteSecretDuringDeleting()
		{
			_encryptingRsapiClientLibrary.Delete(409839);
			_encryptingRsapiClientLibrary.Delete(new[] {172308});
			_encryptingRsapiClientLibrary.Delete(new IntegrationPoint());
			_encryptingRsapiClientLibrary.Delete(new[] {new IntegrationPoint()});

			_secretCatalog.Received(0).RevokeSecret(Arg.Any<SecretRef>());
		}

		[Test]
		public void ItShouldIgnoreMissingSecuredConfiguration()
		{
			var rdo = new IntegrationPoint();

			_secretManager.When(x => x.CreateSecretData(rdo)).Do(y =>
			{
				var a = rdo.SecuredConfiguration;
			});
			_secretManager.When(x => x.RetrieveIdentifier(rdo)).Do(y =>
			{
				var a = rdo.SecuredConfiguration;
			});

			var artifactId = 946199;
			_library.Read(artifactId).Returns(rdo);

			Assert.That(_encryptingRsapiClientLibrary.Read(artifactId), Is.EqualTo(rdo));

			var query = new Query<RDO>();
			_library.Query(query, 0).Returns(new[] {rdo}.ToList());

			Assert.That(_encryptingRsapiClientLibrary.Query(query)[0], Is.EqualTo(rdo));

			_encryptingRsapiClientLibrary.Create(rdo);
			_library.Received(1).Create(rdo);

			_encryptingRsapiClientLibrary.Update(rdo);
			_library.Received(1).Update(rdo);
		}

		[Test]
		public void ItShouldNotIgnoreExceptions()
		{
			var rdo = new IntegrationPoint
			{
				SecuredConfiguration = "secret"
			};

			_secretManager.When(x => x.CreateSecretData(rdo)).Do(y =>
			{
				throw new Exception();
			});
			_secretManager.When(x => x.RetrieveIdentifier(rdo)).Do(y =>
			{
				throw new Exception();
			});

			var artifactId = 483903;
			_library.Read(artifactId).Returns(rdo);

			Assert.That(() => _encryptingRsapiClientLibrary.Read(artifactId), Throws.Exception);

			var query = new Query<RDO>();
			_library.Query(query, 0).Returns(new[] { rdo }.ToList());

			Assert.That(() => _encryptingRsapiClientLibrary.Query(query)[0], Throws.Exception);

			Assert.That(() => _encryptingRsapiClientLibrary.Create(rdo), Throws.Exception);
			Assert.That(() => _encryptingRsapiClientLibrary.Update(rdo), Throws.Exception);
		}
	}
}