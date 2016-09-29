using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.EventHandlers.MassOperations;
using kCura.Relativity.Client;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.MassOperations
{
	[TestFixture]
	public class IntegrationPointMassCopyTests
	{
		private IntegrationPointMassCopy _massCopy;
		private IRSAPIService _service;
		private IIntegrationPointNameHelper _nameHelper;

		[SetUp]
		public void SetUp()
		{
			_service = Substitute.For<IRSAPIService>();
			_nameHelper = Substitute.For<IIntegrationPointNameHelper>();

			_massCopy = new IntegrationPointMassCopy(_service, _nameHelper);
		}

		[Test]
		public void ItShouldSetArtifactIdTo0()
		{
			Data.IntegrationPoint ip = MassCopyIntegrationPointHelper.CreateExampleIntegrationPoint();

			_service.IntegrationPointLibrary.Read(Arg.Any<IEnumerable<int>>()).Returns(new List<Data.IntegrationPoint> {ip});

			_massCopy.Copy(new List<int> {1});

			_service.IntegrationPointLibrary.Received().Create(Arg.Is<Data.IntegrationPoint>(x => x.ArtifactId == 0));
		}

		[Test]
		public void ItShouldDisableScheduler()
		{
			Data.IntegrationPoint ip = MassCopyIntegrationPointHelper.CreateExampleIntegrationPoint();

			_service.IntegrationPointLibrary.Read(Arg.Any<IEnumerable<int>>()).Returns(new List<Data.IntegrationPoint> {ip});

			_massCopy.Copy(new List<int> {1});

			_service.IntegrationPointLibrary.Received().Create(Arg.Is<Data.IntegrationPoint>(x => x.EnableScheduler == false));
		}

		[Test]
		public void ItShouldClearErrors()
		{
			Data.IntegrationPoint ip = MassCopyIntegrationPointHelper.CreateExampleIntegrationPoint();

			_service.IntegrationPointLibrary.Read(Arg.Any<IEnumerable<int>>()).Returns(new List<Data.IntegrationPoint> {ip});

			_massCopy.Copy(new List<int> {1});

			_service.IntegrationPointLibrary.Received().Create(Arg.Is<Data.IntegrationPoint>(x => x.HasErrors == false));
		}


		[Test]
		public void ItShouldSetNameReturnedFromNameHelper()
		{
			const string ipName = "Custom IP Name";
			var expectedName = $"{ipName} (1)";

			Data.IntegrationPoint ip = MassCopyIntegrationPointHelper.CreateExampleIntegrationPoint();
			MassCopyIntegrationPointHelper.MockIntegrationPointName(ip, ipName);

			_service.IntegrationPointLibrary.Read(Arg.Any<IEnumerable<int>>()).Returns(new List<Data.IntegrationPoint> {ip});
			_nameHelper.CreateNameForCopy(ip).Returns(expectedName);

			_massCopy.Copy(new List<int> {1});

			_service.IntegrationPointLibrary.Received().Create(Arg.Is<Data.IntegrationPoint>(x => x.Name.Equals(expectedName)));
		}

		[Test]
		public void ItShouldCopyIntegrationPointConfiguration()
		{
			const bool logErrors = false;
			const int destinationProvider = 123;
			const int sourceProvider = 321;
			const string destinationConfiguration = "expected_destination_configuration";
			const string sourceConfiguration = "expected_source_configuration";
			const string fieldMappings = "expected_field_mappings";
			const string emailNotificationRecipients = "expected_email_notification";
			var overwriteFields = new Choice(Guid.Empty, "Append/Overwrite");

			Data.IntegrationPoint ip = MassCopyIntegrationPointHelper.CreateIntegrationPoint(logErrors, "name", destinationProvider, destinationConfiguration, sourceConfiguration, fieldMappings,
				emailNotificationRecipients, sourceProvider, overwriteFields);

			_service.IntegrationPointLibrary.Read(Arg.Any<IEnumerable<int>>()).Returns(new List<Data.IntegrationPoint> {ip});

			Data.IntegrationPoint actualIntegrationPoint = null;
			_service.IntegrationPointLibrary.Create(Arg.Do<Data.IntegrationPoint>(x => actualIntegrationPoint = x));

			_massCopy.Copy(new List<int> {1});

			Assert.IsNotNull(actualIntegrationPoint);
			Assert.AreEqual(logErrors, actualIntegrationPoint.LogErrors);
			Assert.AreEqual(destinationProvider, actualIntegrationPoint.DestinationProvider);
			Assert.AreEqual(sourceProvider, actualIntegrationPoint.SourceProvider);
			Assert.AreEqual(destinationConfiguration, actualIntegrationPoint.DestinationConfiguration);
			Assert.AreEqual(sourceConfiguration, actualIntegrationPoint.SourceConfiguration);
			Assert.AreEqual(fieldMappings, actualIntegrationPoint.FieldMappings);
			Assert.AreEqual(emailNotificationRecipients, actualIntegrationPoint.EmailNotificationRecipients);
		}

		[Test]
		public void ItShouldReadAllIntegrationPointsAtOnce()
		{
			var ids = new List<int> {123, 456, 789, 100};

			_service.IntegrationPointLibrary.Read(Arg.Any<IEnumerable<int>>()).Returns(new List<Data.IntegrationPoint>());

			_massCopy.Copy(ids);

			_service.IntegrationPointLibrary.Received(1).Read(ids);
		}

		[Test]
		public void ItShouldCopyAllIntegrationPoints()
		{
			var ids = new List<int> {123, 432, 537, 91, 345};

			var ips = new List<Data.IntegrationPoint>();
			for (var i = 0; i < ids.Count; i++)
				ips.Add(MassCopyIntegrationPointHelper.CreateExampleIntegrationPoint());

			_service.IntegrationPointLibrary.Read(Arg.Any<IEnumerable<int>>()).Returns(ips);

			_massCopy.Copy(ids);

			_service.IntegrationPointLibrary.Received(ids.Count).Create(Arg.Any<Data.IntegrationPoint>());
		}
	}
}