using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.Storage;
using IConfiguration = Relativity.Sync.Storage.IConfiguration;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public class ValidationConfigurationTests
	{
		private ValidationConfiguration _configuration;
		private Mock<IConfiguration> _cache;
		private Mock<IFieldMappings> _fieldMappings;
		private const int _WORKSPACE_ID = 111;

		private static readonly Guid DestinationFolderStructureBehaviorGuid = new Guid("A1593105-BD99-4A15-A51A-3AA8D4195908");
		private static readonly Guid DestinationWorkspaceArtifactIdGuid = new Guid("15B88438-6CF7-47AB-B630-424633159C69");
		private static readonly Guid DataDestinationArtifactIdGuid = new Guid("0E9D7B8E-4643-41CC-9B07-3A66C98248A1");
		private static readonly Guid DataSourceArtifactIdGuid = new Guid("6D8631F9-0EA1-4EB9-B7B2-C552F43959D0");
		private static readonly Guid EmailNotificationRecipientsGuid = new Guid("4F03914D-9E86-4B72-B75C-EE48FEEBB583");
		private static readonly Guid FieldOverlayBehaviorGuid = new Guid("34ECB263-1370-4D6C-AC11-558447504EC4");
		private static readonly Guid FolderPathSourceFieldArtifactIdGuid = new Guid("BF5F07A3-6349-47EE-9618-1DD32C9FD998");
		private static readonly Guid ImportOverwriteModeGuid = new Guid("1914D2A3-A1FF-480B-81DC-7A2AA563047A");
		private static readonly Guid JobHistoryGuid = new Guid("5D8F7F01-25CF-4246-B2E2-C05882539BB2");

		[SetUp]
		public void SetUp()
		{
			_cache = new Mock<IConfiguration>();
			_fieldMappings = new Mock<IFieldMappings>();
			SyncJobParameters jobParameters = new SyncJobParameters(1, _WORKSPACE_ID);
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
			_cache.Setup(x => x.GetFieldValue<int>(DestinationWorkspaceArtifactIdGuid)).Returns(expected);
			_configuration.DestinationWorkspaceArtifactId.Should().Be(expected);
		}

		[Test]
		public void ItShouldRetrieveDataDestinationArtifactId()
		{
			const int expected = 1;
			_cache.Setup(x => x.GetFieldValue<int>(DataDestinationArtifactIdGuid)).Returns(expected);
			_configuration.DestinationFolderArtifactId.Should().Be(expected);
		}

		[Test]
		public void ItShouldRetrieveDataSourceArtifactId()
		{
			const int expected = 1;
			_cache.Setup(x => x.GetFieldValue<int>(DataSourceArtifactIdGuid)).Returns(expected);
			_configuration.SavedSearchArtifactId.Should().Be(expected);
		}

		[Test]
		public void ItShouldRetrieveEmailNotificationRecipients()
		{
			const string expected = "email1@example.com;email2@example.com";
			_cache.Setup(x => x.GetFieldValue<string>(EmailNotificationRecipientsGuid)).Returns(expected);
			_configuration.NotificationEmails.Should().Be(expected);
		}

		[Test]
		public void ItShouldRetrieveFieldMappings()
		{
			List<FieldMap> fieldMappings = new List<FieldMap>();
			_fieldMappings.Setup(x => x.GetFieldMappings()).Returns(fieldMappings);
			_configuration.FieldMappings.Should().BeSameAs(fieldMappings);
		}

		[Test]
		public void ItShouldRetrieveFieldOverlayBehavior()
		{
			const FieldOverlayBehavior expected = FieldOverlayBehavior.Default;
			_cache.Setup(x => x.GetFieldValue<string>(FieldOverlayBehaviorGuid)).Returns("Default");
			_configuration.FieldOverlayBehavior.Should().Be(expected);
		}

		[Test]
		public void ItShouldRetrieveFolderPathSourceFieldArtifactId()
		{
			const int expected = 1;
			_cache.Setup(x => x.GetFieldValue<int>(FolderPathSourceFieldArtifactIdGuid)).Returns(expected);
			_configuration.FolderPathSourceFieldArtifactId.Should().Be(expected);
		}

		[Test]
		public void ItShouldRetrieveImportOverwriteMode()
		{
			ImportOverwriteMode expected = ImportOverwriteMode.AppendOverlay;
			_cache.Setup(x => x.GetFieldValue<string>(ImportOverwriteModeGuid)).Returns("AppendOverlay");
			_configuration.ImportOverwriteMode.Should().Be(expected);
		}

		[Test]
		public void ItShouldRetrieveDestinationFolderStructureBehavior()
		{
			DestinationFolderStructureBehavior expected = DestinationFolderStructureBehavior.ReadFromField;
			_cache.Setup(x => x.GetFieldValue<string>(DestinationFolderStructureBehaviorGuid)).Returns("ReadFromField");
			_configuration.DestinationFolderStructureBehavior.Should().Be(expected);
		}

		[Test]
		public void ItShouldRetrieveJobHistory()
		{
			const string jobName = "Job name";
			_cache.Setup(x => x.GetFieldValue<RelativityObjectValue>(JobHistoryGuid)).Returns(new RelativityObjectValue {Name = jobName});
			_configuration.JobName.Should().Be(jobName);
		}
	}
}