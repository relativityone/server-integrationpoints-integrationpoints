using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.RDOs;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Storage
{
	[TestFixture]
	public class ValidationConfigurationTests
	{
		private ValidationConfiguration _configuration;
		private Mock<Sync.Storage.IConfiguration> _cache;
		private Mock<IFieldMappings> _fieldMappings;
		private const int _WORKSPACE_ID = 111;
		
		private static readonly Guid JobHistoryGuid = new Guid("5D8F7F01-25CF-4246-B2E2-C05882539BB2");

		[SetUp]
		public void SetUp()
		{
			_cache = new Mock<Sync.Storage.IConfiguration>();
			_fieldMappings = new Mock<IFieldMappings>();
			SyncJobParameters jobParameters = new SyncJobParameters(1, _WORKSPACE_ID, 1);
			_configuration = new ValidationConfiguration(_cache.Object, _fieldMappings.Object, jobParameters);
		}

		[Test]
		public void ItShouldRetrieveSourceWorkspaceArtifactId()
		{
			_configuration.SourceWorkspaceArtifactId.Should().Be(_WORKSPACE_ID);
		}

		[Test]
		public void ItShouldRetrieveDestinationWorkspaceArtifactId()
		{
			const int expected = 1;
			_cache.Setup(x => x.GetFieldValue<int>(SyncConfigurationRdo.DestinationWorkspaceArtifactIdGuid)).Returns(expected);
			_configuration.DestinationWorkspaceArtifactId.Should().Be(expected);
		}

		[Test]
		public void ItShouldRetrieveDataDestinationArtifactId()
		{
			const int expected = 1;
			_cache.Setup(x => x.GetFieldValue<int>(SyncConfigurationRdo.DataDestinationArtifactIdGuid)).Returns(expected);
			_configuration.DestinationFolderArtifactId.Should().Be(expected);
		}

		[Test]
		public void ItShouldRetrieveDataSourceArtifactId()
		{
			const int expected = 1;
			_cache.Setup(x => x.GetFieldValue<int>(SyncConfigurationRdo.DataSourceArtifactIdGuid)).Returns(expected);
			_configuration.SavedSearchArtifactId.Should().Be(expected);
		}

		[Test]
		public void ItShouldRetrieveEmailNotificationRecipients()
		{
			const string expected = "email1@example.com;email2@example.com";
			_cache.Setup(x => x.GetFieldValue<string>(SyncConfigurationRdo.EmailNotificationRecipientsGuid)).Returns(expected);
			_configuration.GetNotificationEmails().Should().Be(expected);
		}

		[Test]
		public void ItShouldRetrieveFieldMappings()
		{
			List<FieldMap> fieldMappings = new List<FieldMap>();
			_fieldMappings.Setup(x => x.GetFieldMappings()).Returns(fieldMappings);
			_configuration.GetFieldMappings().Should().BeSameAs(fieldMappings);
		}

		[Test]
		public void ItShouldRetrieveFieldOverlayBehavior()
		{
			const FieldOverlayBehavior expected = FieldOverlayBehavior.UseFieldSettings;
			_cache.Setup(x => x.GetFieldValue<string>(SyncConfigurationRdo.FieldOverlayBehaviorGuid)).Returns("Use Field Settings");
			_configuration.FieldOverlayBehavior.Should().Be(expected);
		}

		[Test]
		public void ItShouldRetrieveFolderPathSourceFieldArtifactId()
		{
			const string expected = "name";
			_cache.Setup(x => x.GetFieldValue<string>(SyncConfigurationRdo.FolderPathSourceFieldNameGuid)).Returns(expected);
			_configuration.GetFolderPathSourceFieldName().Should().Be(expected);
		}

		[Test]
		public void ItShouldRetrieveImportOverwriteMode()
		{
			ImportOverwriteMode expected = ImportOverwriteMode.AppendOverlay;
			_cache.Setup(x => x.GetFieldValue<string>(SyncConfigurationRdo.ImportOverwriteModeGuid)).Returns("AppendOverlay");
			_configuration.ImportOverwriteMode.Should().Be(expected);
		}

		[Test]
		public void ItShouldRetrieveDestinationFolderStructureBehavior()
		{
			DestinationFolderStructureBehavior expected = DestinationFolderStructureBehavior.ReadFromField;
			_cache.Setup(x => x.GetFieldValue<string>(SyncConfigurationRdo.DestinationFolderStructureBehaviorGuid)).Returns("ReadFromField");
			_configuration.DestinationFolderStructureBehavior.Should().Be(expected);
		}

		[Test]
		public void ItShouldRetrieveJobHistory()
		{
			const string jobName = "Job name";
			_cache.Setup(x => x.GetFieldValue<RelativityObjectValue>(JobHistoryGuid)).Returns(new RelativityObjectValue {Name = jobName});
			_configuration.GetJobName().Should().Be(jobName);
		}
	}
}