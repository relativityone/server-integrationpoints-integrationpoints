﻿using kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Repositories;
using Moq;
using NUnit.Framework;
using Relativity;
using Relativity.Core;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Interfaces.ViewField.Models;
using ViewFieldInfo = kCura.WinEDDS.ViewFieldInfo;
using ColumnSourceType = Relativity.ViewFieldInfo.ColumnSourceType;
using FieldType = Relativity.FieldTypeHelper.FieldType;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.ExportManagers
{
	[TestFixture]
	public class CoreSearchManagerTests
	{
		private Mock<BaseServiceContext> _baseServiceContextMock;
		private Mock<IViewFieldRepository> _viewFieldRepositoryMock;

		private const int _WORKSPACE_ID = 1001000;
		private const int _ARTIFACT_ID = 1000100;
		private const int _ARTIFACT_ID_2 = 1002100;
		private const int _ARTIFACT_VIEW_FIELD_ID = 1000200;
		private const int _ARTIFACT_VIEW_FIELD_ID_2 = 1002200;
		private const FieldCategoryEnum _CATEGORY = FieldCategoryEnum.Batch;
		private const FieldCategory _CATEGORY_CONVERTED = FieldCategory.Batch;
		private const string _DISPLAY_NAME = "DisplayName";
		private const string _ARTIFACT_VIEW_FIELD_COLUMN_NAME = "AvfColumnName";
		private const string _ARTIFACT_VIEW_FIELD_HEADER_NAME = "AvfHeaderName";
		private const string _ALLOW_FIELD_NAME = "AllowFieldName";
		private const ColumnSourceTypeEnum _COLUMN_SOURCE_TYPE = ColumnSourceTypeEnum.Computed;
		private const ColumnSourceType _COLUMN_SOURCE_TYPE_CONVERTED = ColumnSourceType.Computed;
		private const string _DATA_SOURCE = "DataSource";
		private const string _SOURCE_FIELD_NAME = "SourceFieldName";
		private const int _SOURCE_FIELD_ARTIFACT_TYPE_ID = 10;
		private const int _SOURCE_FIELD_ARTIFACT_ID = 10004300;
		private const int _CONNECTOR_FIELD_ARTIFACT_ID = 1000400;
		private const string _SOURCE_FIELD_ARTIFACT_TYPE_TABLE_NAME = "SourceFieldArtifactTypeTableName";
		private const string _CONNECTOR_FIELD_NAME = "ConnectorFieldName";
		private const FieldCategoryEnum _CONNECTOR_FIELD_CATEGORY = FieldCategoryEnum.Comments;
		private const FieldCategory _CONNECTOR_FIELD_CATEGORY_CONVERTED = FieldCategory.Comments;
		private const FieldTypeEnum _FIELD_TYPE = FieldTypeEnum.Boolean;
		private const FieldType _FIELD_TYPE_CONVERTED = FieldType.Boolean;
		private const bool _IS_LINKED = false;
		private const int _FIELD_CODE_TYPE_ID = 11;
		private const int _ARTIFACT_TYPE_ID = 12;
		private const string _ARTIFACT_TYPE_TABLE_NAME = "ArtifactTypeTableName";
		private const bool _FIELD_IS_ARTIFACT_BASE_FIELD = true;
		private const string _FORMAT_STRING = "FormatString";
		private const bool _IS_UNICODE_ENABLED = false;
		private const bool _ALLOW_HTML = true;
		private const int _PARENT_FILE_FIELD_ARTIFACT_ID = 1000500;
		private const string _PARENT_FILE_FIELD_DISPLAY_NAME = "ParentFileFieldDisplayName";
		private const int _ASSOCIATIVE_ARTIFACT_TYPE_ID = 13;
		private const string _RELATIONAL_TABLE_NAME = "RelationalTableName";
		private const string _RELATIONAL_TABLE_COLUMN_NAME = "RelationalTableColumnName";
		private const string _RELATIONAL_TABLE_COLUMN_NAME_2 = "RelationalTableColumnName2";
		private const ParentReflectionTypeEnum _PARENT_REFLECTION_TYPE = ParentReflectionTypeEnum.GrandParent;
		private const ParentReflectionType _PARENT_REFLECTION_TYPE_CONVERTED = ParentReflectionType.GrandParent;
		private const string _REFLECTED_FIELD_ARTIFACT_TYPE_TABLE_NAME = "ReflectedFieldArtifactTypeTableName";
		private const string _REFLECTED_FIELD_IDENTIFIER_COLUMN_NAME = "ReflectedFieldIdentifierColumnName";
		private const string _REFLECTED_FIELD_CONNECTOR_FIELD_NAME = "ReflectedFieldConnectorFieldName";
		private const string _REFLECTED_CONNECTOR_IDENTIFIER_COLUMN_NAME = "ReflectedConnectorIdentifierColumnName";
		private const bool _ENABLE_DATA_GRID = false;
		private const bool _IS_VIRTUAL_ASSOCIATIVE_ARTIFACT_TYPE = true;

		[SetUp]
		public void SetUp()
		{
			_baseServiceContextMock = new Mock<BaseServiceContext>();
			_viewFieldRepositoryMock = new Mock<IViewFieldRepository>();
		}

		[Test]
		public void RetrieveDefaultViewFieldIdsForSavedSearchTest()
		{
			// arrange
			ViewFieldIDResponse viewFieldIDResponse1 = CreateTestViewFieldIDResponse(_ARTIFACT_ID, _ARTIFACT_VIEW_FIELD_ID);
			ViewFieldIDResponse viewFieldIdResponse2 = CreateTestViewFieldIDResponse(_ARTIFACT_ID_2, _ARTIFACT_VIEW_FIELD_ID_2);
			ViewFieldIDResponse[] viewFieldIDResponseArray = { viewFieldIDResponse1, viewFieldIdResponse2 };
			_viewFieldRepositoryMock
				.Setup(x => x.ReadViewFieldIDsFromSearch(_WORKSPACE_ID, _ARTIFACT_TYPE_ID, _ARTIFACT_ID))
				.Returns(viewFieldIDResponseArray);

			var coreSearchManager = new CoreSearchManager(_baseServiceContextMock.Object, _viewFieldRepositoryMock.Object);

			// act
			int[] result = coreSearchManager.RetrieveDefaultViewFieldIds(_WORKSPACE_ID, _ARTIFACT_ID, _ARTIFACT_TYPE_ID, false);

			// assert
			_viewFieldRepositoryMock.Verify(
				x => x.ReadViewFieldIDsFromSearch(_WORKSPACE_ID, _ARTIFACT_TYPE_ID, _ARTIFACT_ID), Times.Once);
			Assert.AreEqual(1, result.Length);
			Assert.AreEqual(_ARTIFACT_VIEW_FIELD_ID, result[0]);
		}

		[Test]
		public void RetrieveDefaultViewFieldIdsForProductionTest()
		{
			// arrange
			ViewFieldIDResponse viewFieldIDResponse1 = CreateTestViewFieldIDResponse(_ARTIFACT_ID, _ARTIFACT_VIEW_FIELD_ID);
			ViewFieldIDResponse viewFieldIdResponse2 = CreateTestViewFieldIDResponse(_ARTIFACT_ID_2, _ARTIFACT_VIEW_FIELD_ID_2);
			ViewFieldIDResponse[] viewFieldIDResponseArray = { viewFieldIDResponse1, viewFieldIdResponse2 };
			_viewFieldRepositoryMock
				.Setup(x => x.ReadViewFieldIDsFromProduction(_WORKSPACE_ID, _ARTIFACT_TYPE_ID, _ARTIFACT_ID))
				.Returns(viewFieldIDResponseArray);

			var coreSearchManager = new CoreSearchManager(_baseServiceContextMock.Object, _viewFieldRepositoryMock.Object);

			// act
			int[] result = coreSearchManager.RetrieveDefaultViewFieldIds(_WORKSPACE_ID, _ARTIFACT_ID, _ARTIFACT_TYPE_ID, true);

			// assert
			_viewFieldRepositoryMock.Verify(
				x => x.ReadViewFieldIDsFromProduction(_WORKSPACE_ID, _ARTIFACT_TYPE_ID, _ARTIFACT_ID), Times.Once);
			Assert.AreEqual(1, result.Length);
			Assert.AreEqual(_ARTIFACT_VIEW_FIELD_ID, result[0]);
		}

		private static ViewFieldIDResponse CreateTestViewFieldIDResponse(int artifactID, int artifactViewFieldID)
		{
			var viewFieldIDResponse = new ViewFieldIDResponse
			{
				ArtifactID = artifactID,
				ArtifactViewFieldID = artifactViewFieldID
			};
			return viewFieldIDResponse;
		}

		[Test]
		public void RetrieveAllExportableViewFieldsTest()
		{
			// arrange
			ViewFieldResponse viewFieldResponse = CreateTestViewFieldResponse();
			ViewFieldResponse[] viewFieldResponseArray = {viewFieldResponse};
			_viewFieldRepositoryMock.Setup(x => x.ReadExportableViewFields(_WORKSPACE_ID, _ARTIFACT_TYPE_ID))
				.Returns(viewFieldResponseArray);
			var coreSearchManager = new CoreSearchManager(_baseServiceContextMock.Object, _viewFieldRepositoryMock.Object);

			// act
			ViewFieldInfo[] result = coreSearchManager.RetrieveAllExportableViewFields(_WORKSPACE_ID, _ARTIFACT_TYPE_ID);

			// assert
			_viewFieldRepositoryMock.Verify(x => x.ReadExportableViewFields(_WORKSPACE_ID, _ARTIFACT_TYPE_ID), Times.Once);
			Assert.AreEqual(1, result.Length);
			ValidateViewFieldInfo(result[0]);
		}

		private static ViewFieldResponse CreateTestViewFieldResponse()
		{
			var viewFieldResponse = new ViewFieldResponse
			{
				ArtifactID = _ARTIFACT_ID,
				ArtifactViewFieldID = _ARTIFACT_VIEW_FIELD_ID,
				Category = _CATEGORY,
				DisplayName = _DISPLAY_NAME,
				ArtifactViewFieldColumnName = _ARTIFACT_VIEW_FIELD_COLUMN_NAME,
				ArtifactViewFieldHeaderName = _ARTIFACT_VIEW_FIELD_HEADER_NAME,
				AllowFieldName = _ALLOW_FIELD_NAME,
				ColumnSourceType = _COLUMN_SOURCE_TYPE,
				DataSource = _DATA_SOURCE,
				SourceFieldName = _SOURCE_FIELD_NAME,
				SourceFieldArtifactTypeID = _SOURCE_FIELD_ARTIFACT_TYPE_ID,
				SourceFieldArtifactID = _SOURCE_FIELD_ARTIFACT_ID,
				ConnectorFieldArtifactID = _CONNECTOR_FIELD_ARTIFACT_ID,
				SourceFieldArtifactTypeTableName = _SOURCE_FIELD_ARTIFACT_TYPE_TABLE_NAME,
				ConnectorFieldName = _CONNECTOR_FIELD_NAME,
				ConnectorFieldCategory = _CONNECTOR_FIELD_CATEGORY,
				FieldType = _FIELD_TYPE,
				IsLinked = _IS_LINKED,
				FieldCodeTypeID = _FIELD_CODE_TYPE_ID,
				ArtifactTypeID = _ARTIFACT_TYPE_ID,
				ArtifactTypeTableName = _ARTIFACT_TYPE_TABLE_NAME,
				FieldIsArtifactBaseField = _FIELD_IS_ARTIFACT_BASE_FIELD,
				FormatString = _FORMAT_STRING,
				IsUnicodeEnabled = _IS_UNICODE_ENABLED,
				AllowHtml = _ALLOW_HTML,
				ParentFileFieldArtifactID = _PARENT_FILE_FIELD_ARTIFACT_ID,
				ParentFileFieldDisplayName = _PARENT_FILE_FIELD_DISPLAY_NAME,
				AssociativeArtifactTypeID = _ASSOCIATIVE_ARTIFACT_TYPE_ID,
				RelationalTableName = _RELATIONAL_TABLE_NAME,
				RelationalTableColumnName = _RELATIONAL_TABLE_COLUMN_NAME,
				RelationalTableColumnName2 = _RELATIONAL_TABLE_COLUMN_NAME_2,
				ParentReflectionType = _PARENT_REFLECTION_TYPE,
				ReflectedFieldArtifactTypeTableName = _REFLECTED_FIELD_ARTIFACT_TYPE_TABLE_NAME,
				ReflectedFieldIdentifierColumnName = _REFLECTED_FIELD_IDENTIFIER_COLUMN_NAME,
				ReflectedFieldConnectorFieldName = _REFLECTED_FIELD_CONNECTOR_FIELD_NAME,
				ReflectedConnectorIdentifierColumnName = _REFLECTED_CONNECTOR_IDENTIFIER_COLUMN_NAME,
				EnableDataGrid = _ENABLE_DATA_GRID,
				IsVirtualAssociativeArtifactType = _IS_VIRTUAL_ASSOCIATIVE_ARTIFACT_TYPE
			};
			return viewFieldResponse;
		}

		private static void ValidateViewFieldInfo(ViewFieldInfo viewFieldInfo)
		{
			Assert.AreEqual(_ARTIFACT_ID, viewFieldInfo.FieldArtifactId);
			Assert.AreEqual(_ARTIFACT_VIEW_FIELD_ID, viewFieldInfo.AvfId);
			Assert.AreEqual(_CATEGORY_CONVERTED, viewFieldInfo.Category);
			Assert.AreEqual(_DISPLAY_NAME, viewFieldInfo.DisplayName);
			Assert.AreEqual(_ARTIFACT_VIEW_FIELD_COLUMN_NAME, viewFieldInfo.AvfColumnName);
			Assert.AreEqual(_ARTIFACT_VIEW_FIELD_HEADER_NAME, viewFieldInfo.AvfHeaderName);
			Assert.AreEqual(_ALLOW_FIELD_NAME, viewFieldInfo.AllowFieldName);
			Assert.AreEqual(_COLUMN_SOURCE_TYPE_CONVERTED, viewFieldInfo.ColumnSource);
			Assert.AreEqual(_DATA_SOURCE, viewFieldInfo.DataSource);
			Assert.AreEqual(_SOURCE_FIELD_NAME, viewFieldInfo.SourceFieldName);
			Assert.AreEqual(_SOURCE_FIELD_ARTIFACT_TYPE_ID, viewFieldInfo.SourceFieldArtifactTypeID);
			Assert.AreEqual(_SOURCE_FIELD_ARTIFACT_ID, viewFieldInfo.SourceFieldArtifactID);
			Assert.AreEqual(_CONNECTOR_FIELD_ARTIFACT_ID, viewFieldInfo.ConnectorFieldArtifactID);
			Assert.AreEqual(_SOURCE_FIELD_ARTIFACT_TYPE_TABLE_NAME, viewFieldInfo.SourceFieldArtifactTypeTableName);
			Assert.AreEqual(_CONNECTOR_FIELD_NAME, viewFieldInfo.ConnectorFieldName);
			Assert.AreEqual(_CONNECTOR_FIELD_CATEGORY_CONVERTED, viewFieldInfo.ConnectorFieldCategory);
			Assert.AreEqual(_FIELD_TYPE_CONVERTED, viewFieldInfo.FieldType);
			Assert.AreEqual(_IS_LINKED, viewFieldInfo.IsLinked);
			Assert.AreEqual(_FIELD_CODE_TYPE_ID, viewFieldInfo.FieldCodeTypeID);
			Assert.AreEqual(_ARTIFACT_TYPE_ID, viewFieldInfo.ArtifactTypeID);
			Assert.AreEqual(_ARTIFACT_TYPE_TABLE_NAME, viewFieldInfo.ArtifactTypeTableName);
			Assert.AreEqual(_FIELD_IS_ARTIFACT_BASE_FIELD, viewFieldInfo.FieldIsArtifactBaseField);
			Assert.AreEqual(_FORMAT_STRING, viewFieldInfo.FormatString);
			Assert.AreEqual(_IS_UNICODE_ENABLED, viewFieldInfo.IsUnicodeEnabled);
			Assert.AreEqual(_ALLOW_HTML, viewFieldInfo.AllowHtml);
			Assert.AreEqual(_PARENT_FILE_FIELD_ARTIFACT_ID, viewFieldInfo.ParentFileFieldArtifactID);
			Assert.AreEqual(_PARENT_FILE_FIELD_DISPLAY_NAME, viewFieldInfo.ParentFileFieldDisplayName);
			Assert.AreEqual(_ASSOCIATIVE_ARTIFACT_TYPE_ID, viewFieldInfo.AssociativeArtifactTypeID);
			Assert.AreEqual(_RELATIONAL_TABLE_NAME, viewFieldInfo.RelationalTableName);
			Assert.AreEqual(_RELATIONAL_TABLE_COLUMN_NAME, viewFieldInfo.RelationalTableColumnName);
			Assert.AreEqual(_RELATIONAL_TABLE_COLUMN_NAME_2, viewFieldInfo.RelationalTableColumnName2);
			Assert.AreEqual(_PARENT_REFLECTION_TYPE_CONVERTED, viewFieldInfo.ParentReflectionType);
			Assert.AreEqual(_REFLECTED_FIELD_ARTIFACT_TYPE_TABLE_NAME, viewFieldInfo.ReflectedFieldArtifactTypeTableName);
			Assert.AreEqual(_REFLECTED_FIELD_IDENTIFIER_COLUMN_NAME, viewFieldInfo.ReflectedFieldIdentifierColumnName);
			Assert.AreEqual(_REFLECTED_FIELD_CONNECTOR_FIELD_NAME, viewFieldInfo.ReflectedFieldConnectorFieldName);
			Assert.AreEqual(_REFLECTED_CONNECTOR_IDENTIFIER_COLUMN_NAME, viewFieldInfo.ReflectedConnectorIdentifierColumnName);
			Assert.AreEqual(_ENABLE_DATA_GRID, viewFieldInfo.EnableDataGrid);
			Assert.AreEqual(_IS_VIRTUAL_ASSOCIATIVE_ARTIFACT_TYPE, viewFieldInfo.IsVirtualAssociativeArtifactType);
		}
	}
}