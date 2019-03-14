using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Repositories
{
	[TestFixture]
	public class IntegrationPointRepositoryTests
	{
		private Mock<IRelativityObjectManager> _objectManager;
		private IntegrationPoint _integrationPoint;
		private Stream _fieldMappingsStream;
		private Guid _guid = Guid.Parse(IntegrationPointFieldGuids.FieldMappings);
		private DateTime _nextScheduledRuntime;
		private DateTime _lastRuntime;

		private const int _SOURCE_PROVIDER = 2;
		private const int _DESTINATION_PROVIDER = 4;
		private const int _TYPE = 3;
		private const int _JOB_HISTORY_1 = 12;
		private const int _JOB_HISTORY_2 = 15;
		private const int _ARTIFACT_ID = 1025823;
		private const string _FIELD_MAPPINGS_LONG = "fieldMappingsLong";
		private const string _SECURED_CONFIGURATION = "securedConf";
		private const string _NAME = "Test Integration Point";

		[SetUp]
		public void SetUp()
		{
			_objectManager = new Mock<IRelativityObjectManager>();
			_fieldMappingsStream = GenerateStreamFromString(_FIELD_MAPPINGS_LONG);
			_nextScheduledRuntime = DateTime.Now.AddDays(1);
			_lastRuntime = DateTime.Now.AddDays(-1);
		}

		private IntegrationPoint CreateTestIntegrationPoint()
		{
			return new IntegrationPoint
			{
				ArtifactId = _ARTIFACT_ID,
				Name = _NAME,
				SourceConfiguration = "sourceConf",
				DestinationConfiguration = "destConf",
				FieldMappings = "fieldMappingsShort",
				SecuredConfiguration = _SECURED_CONFIGURATION,
				SourceProvider = _SOURCE_PROVIDER,
				DestinationProvider = _DESTINATION_PROVIDER,
				Type = _TYPE,
				EmailNotificationRecipients = "admin@relativity.com",
				ScheduleRule = "scheduleRule",
				EnableScheduler = true,
				HasErrors = false,
				LogErrors = true,
				PromoteEligible = false,
				OverwriteFields = null,
				JobHistory = new[] { _JOB_HISTORY_1, _JOB_HISTORY_2 },
				NextScheduledRuntimeUTC = _nextScheduledRuntime,
				LastRuntimeUTC = _lastRuntime
			};
		}

		private static Stream GenerateStreamFromString(string text)
		{
			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			writer.Write(text);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}

		private static bool AreIntegrationPointsEqual(IntegrationPoint integrationPoint1, IntegrationPoint integrationPoint2)
		{
			return integrationPoint1.ArtifactId == integrationPoint2.ArtifactId &&
				   integrationPoint1.Name == integrationPoint2.Name &&
				   integrationPoint1.SourceConfiguration == integrationPoint2.SourceConfiguration &&
				   integrationPoint1.DestinationConfiguration == integrationPoint2.DestinationConfiguration &&
				   integrationPoint1.FieldMappings == integrationPoint2.FieldMappings &&
				   integrationPoint1.SecuredConfiguration == integrationPoint2.SecuredConfiguration &&
				   integrationPoint1.SourceProvider == integrationPoint2.SourceProvider &&
				   integrationPoint1.DestinationProvider == integrationPoint2.DestinationProvider &&
				   integrationPoint1.Type == integrationPoint2.Type &&
				   integrationPoint1.EmailNotificationRecipients == integrationPoint2.EmailNotificationRecipients &&
				   integrationPoint1.ScheduleRule == integrationPoint2.ScheduleRule &&
				   integrationPoint1.EnableScheduler == integrationPoint2.EnableScheduler &&
				   integrationPoint1.HasErrors == integrationPoint2.HasErrors &&
				   integrationPoint1.LogErrors == integrationPoint2.LogErrors &&
				   integrationPoint1.PromoteEligible == integrationPoint2.PromoteEligible &&
				   integrationPoint1.OverwriteFields == integrationPoint2.OverwriteFields &&
				   integrationPoint1.JobHistory.SequenceEqual(integrationPoint2.JobHistory) &&
				   Equals(integrationPoint1.NextScheduledRuntimeUTC, integrationPoint2.NextScheduledRuntimeUTC) &&
				   Equals(integrationPoint1.LastRuntimeUTC, integrationPoint2.LastRuntimeUTC);
		}

		[Test]
		public void ItShouldReturnValidIntegrationPoint()
		{
			// Arrange
			_integrationPoint = CreateTestIntegrationPoint();
			_objectManager.Setup(x => x.Read<IntegrationPoint>(_ARTIFACT_ID, ExecutionIdentity.CurrentUser)).Returns(_integrationPoint);
			_objectManager.Setup(x => x.StreamLongTextAsync(
					_ARTIFACT_ID,
					It.Is<FieldRef>(f => f.Guid.ToString() == _guid.ToString()),
					ExecutionIdentity.CurrentUser))
				.Returns(Task.FromResult(_fieldMappingsStream));
			IntegrationPoint expectedResult = CreateTestIntegrationPoint();
			expectedResult.FieldMappings = _FIELD_MAPPINGS_LONG;
			var integrationPointRepository = new IntegrationPointRepository(_objectManager.Object);

			// Act
			IntegrationPoint actualResult = integrationPointRepository.Read(_ARTIFACT_ID);

			// Assert
			_objectManager.Verify(x => x.Read<IntegrationPoint>(_ARTIFACT_ID, ExecutionIdentity.CurrentUser), Times.Once);
			_objectManager.Verify(
				x => x.StreamLongTextAsync(
					_ARTIFACT_ID,
					It.Is<FieldRef>(f => f.Guid.ToString() == _guid.ToString()),
					ExecutionIdentity.CurrentUser),
				Times.Once());
			Assert.IsTrue(AreIntegrationPointsEqual(expectedResult, actualResult));
		}

		[Test]
		public void ItShouldReturnValidFieldMap()
		{
			// Arrange
			_integrationPoint = CreateTestIntegrationPoint();
			_objectManager.Setup(x => x.StreamLongTextAsync(
					_ARTIFACT_ID,
					It.Is<FieldRef>(f => f.Guid.ToString() == _guid.ToString()),
					ExecutionIdentity.CurrentUser))
				.Returns(Task.FromResult(_fieldMappingsStream));
			var integrationPointRepository = new IntegrationPointRepository(_objectManager.Object);

			// Act
			string actualResult = integrationPointRepository.GetFieldMapJson(_ARTIFACT_ID);

			// Assert
			_objectManager.Verify(
				x => x.StreamLongTextAsync(
					_ARTIFACT_ID,
					It.Is<FieldRef>(f => f.Guid.ToString() == _guid.ToString()),
					ExecutionIdentity.CurrentUser),
				Times.Once());
			Assert.AreEqual(_FIELD_MAPPINGS_LONG, actualResult);
		}

		[Test]
		public void ItShouldReturnValidSecuredConfiguration()
		{
			// Arrange
			_integrationPoint = CreateTestIntegrationPoint();
			_objectManager.Setup(x => x.Read<IntegrationPoint>(_ARTIFACT_ID, ExecutionIdentity.CurrentUser)).Returns(_integrationPoint);
			var integrationPointRepository = new IntegrationPointRepository(_objectManager.Object);

			// Act
			string actualResult = integrationPointRepository.GetSecuredConfiguration(_ARTIFACT_ID);

			// Assert
			_objectManager.Verify(x => x.Read<IntegrationPoint>(_ARTIFACT_ID, ExecutionIdentity.CurrentUser), Times.Once);
			Assert.AreEqual(_SECURED_CONFIGURATION, actualResult);
		}

		[Test]
		public void ItShouldReturnValidName()
		{
			// Arrange
			_integrationPoint = CreateTestIntegrationPoint();
			_objectManager.Setup(x => x.Read<IntegrationPoint>(_ARTIFACT_ID, ExecutionIdentity.CurrentUser)).Returns(_integrationPoint);
			var integrationPointRepository = new IntegrationPointRepository(_objectManager.Object);

			// Act
			string actualResult = integrationPointRepository.GetName(_ARTIFACT_ID);

			// Assert
			_objectManager.Verify(x => x.Read<IntegrationPoint>(_ARTIFACT_ID, ExecutionIdentity.CurrentUser), Times.Once);
			Assert.AreEqual(_NAME, actualResult);
		}
	}
}