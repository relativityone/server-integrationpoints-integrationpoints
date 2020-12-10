using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.RDOs;
using Relativity.Sync.Storage;
using Relativity.Sync.SyncConfiguration;
using Relativity.Sync.SyncConfiguration.Options;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Tests.Unit.SyncConfiguration
{
	[TestFixture]
	public class DocumentSyncConfigurationBuilderTests
	{
		private Mock<ISyncServiceManager> _servicesMgrFake;
		private Mock<IFieldsMappingBuilder> _fieldsMappingBuilderFake;
		private Mock<ISerializer> _serializerFake;

		private const int _SOURCE_WORKSPACE_ID = 10;
		private const int _DESTINATION_WORKSPACE_ID = 20;
		private const int _PARENT_OBJECT_ID = 30;

		private const int _SAVED_SEARCH_ID = 1;
		private const int _DESTINATION_FOLDER_ID = 2;

		private readonly ISyncContext TestSyncContext = new SyncContext(
			_SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID, _PARENT_OBJECT_ID);

		[SetUp]
		public void SetUp()
		{
			_servicesMgrFake = new Mock<ISyncServiceManager>();
			_fieldsMappingBuilderFake = new Mock<IFieldsMappingBuilder>();

			_serializerFake = new Mock<ISerializer>();
		}
		//SyncConfiguration.RdoArtifactTypeId = (int) ArtifactType.Document;
		//SyncConfiguration.DataSourceType = DataSourceType.SavedSearch.ToString(); //ToString ???
		//SyncConfiguration.DestinationWorkspaceArtifactId = SyncContext.DestinationWorkspaceId;
		//SyncConfiguration.DataDestinationType = DestinationLocationType.Folder.ToString(); //ToString ???
		//SyncConfiguration.DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.None.ToString(); //ToString ??? 

		//SyncConfiguration.DataSourceArtifactId = options.SavedSearchId;
		//SyncConfiguration.DataDestinationArtifactId = options.DestinationFolderId;
		//SyncConfiguration.NativesBehavior = options.CopyNativesMode.GetDescription();

		//var fieldsMapping = options.FieldsMapping != null && options.FieldsMapping.Any()
		//	? options.FieldsMapping
		//	: fieldsMappingBuilder.WithIdentifier().FieldsMapping;
		//SyncConfiguration.FieldsMapping = Serializer.Serialize(fieldsMapping);


		[Test]
		public void Build_ShouldInitializeSyncConfiguration_WhenSimpleOptions()
		{
			// Arrange
			List<FieldMap> fieldsMapping = CreateIdentifierOnlyFieldMap();
			const string expectedSerializedFieldMap = "Serialized Identifier FieldsMapping";

			SyncConfigurationRdo expectedConfiguration = new SyncConfigurationRdo
			{
				RdoArtifactTypeId = 10,
				DataSourceType = "SavedSearch",
				DataSourceArtifactId = _SAVED_SEARCH_ID,
				DestinationWorkspaceArtifactId = _DESTINATION_WORKSPACE_ID,
				DataDestinationArtifactId = _DESTINATION_FOLDER_ID,
				DataDestinationType = "Folder",
				DestinationFolderStructureBehavior = "None",
				ImportOverwriteMode = "AppendOnly",
				FieldOverlayBehavior = "Use Field Settings",
				FieldsMapping = expectedSerializedFieldMap,
				NativesBehavior = "None",
				EmailNotificationRecipients = "",
			};
			

			DocumentSyncOptions options = new DocumentSyncOptions(_SAVED_SEARCH_ID, _DESTINATION_FOLDER_ID, fieldsMapping);

			// Act
			DocumentSyncConfigurationBuilder builder = CreateDocumentSyncConfigurationBuilder(options);

			// Assert
			builder.SyncConfiguration.Should().BeEquivalentTo(expectedConfiguration);
		}

		private DocumentSyncConfigurationBuilder CreateDocumentSyncConfigurationBuilder(DocumentSyncOptions options)
		{
			return new DocumentSyncConfigurationBuilder(TestSyncContext, _servicesMgrFake.Object, 
				_fieldsMappingBuilderFake.Object, _serializerFake.Object, options);
		}

		private List<FieldMap> CreateIdentifierOnlyFieldMap()
		{
			return new List<FieldMap>
			{
				new FieldMap()
				{
					SourceField = new FieldEntry {DisplayName = "Identifier", FieldIdentifier = 1, IsIdentifier = true},
					DestinationField = new FieldEntry
						{DisplayName = "Identifier", FieldIdentifier = 1, IsIdentifier = true},
					FieldMapType = FieldMapType.Identifier
				}
			};
		}

		private List<FieldMap> CreateFieldMap()
		{
			return new List<FieldMap>
			{
				new FieldMap()
				{
					SourceField = new FieldEntry {DisplayName = "Identifier", FieldIdentifier = 1, IsIdentifier = true},
					DestinationField = new FieldEntry {DisplayName = "Identifier", FieldIdentifier = 1, IsIdentifier = true},
					FieldMapType = FieldMapType.Identifier
				},
				new FieldMap()
				{
					SourceField = new FieldEntry {DisplayName = "Test", FieldIdentifier = 2, IsIdentifier = true},
					DestinationField = new FieldEntry {DisplayName = "Test", FieldIdentifier = 2, IsIdentifier = true},
					FieldMapType = FieldMapType.None
				}
			};
		}
	}
}
