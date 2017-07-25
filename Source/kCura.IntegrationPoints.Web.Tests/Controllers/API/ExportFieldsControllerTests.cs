using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.Web.DataStructures;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using ExportSettings = kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportSettings;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API
{
	[TestFixture]
	public class ExportFieldsControllerTests : TestBase
	{
		private ExportFieldsController _instance;
		private IExportFieldsService _exportFieldsService;
		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 123951;
		private const int _TRANSFERRED_ARTIFACT_TYPE_ID = 1234;

		[SetUp]
		public override void SetUp()
		{
			_exportFieldsService = Substitute.For<IExportFieldsService>();

			_instance = new ExportFieldsController(_exportFieldsService)
			{
				Request = new HttpRequestMessage()
			};
			_instance.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
		}

		[TestCase(new [] { "DisplayName"}, 0)]
		[TestCase(new [] { "a", "b" }, 0)]
		[TestCase(new [] { "b", "a", "c"}, 0)]
		[TestCase(new [] { "b", "A", "c"}, 0)]
		public void ItShouldGetExportableFields(string[] displayNames, int tmp)
		{
			//Arrange
			var data = new ExtendedSourceOptions()
			{
				TransferredArtifactTypeId = _TRANSFERRED_ARTIFACT_TYPE_ID,
				Options = JsonConvert.SerializeObject(new ExportUsingSavedSearchSettings() {SourceWorkspaceArtifactId = _SOURCE_WORKSPACE_ARTIFACT_ID })
			};
			FieldEntry[] expectedFields = CreateFieldsEntry(displayNames);
			IOrderedEnumerable<FieldEntry> expectedOrderedFieldEntries = expectedFields.OrderBy(x => x.DisplayName);

			_exportFieldsService.GetAllExportableFields(_SOURCE_WORKSPACE_ARTIFACT_ID, _TRANSFERRED_ARTIFACT_TYPE_ID).Returns(expectedFields);

			//Act
			HttpResponseMessage response = _instance.GetExportableFields(data);
			IOrderedEnumerable<FieldEntry> actualFields = ExtractFieldEntryFromResponse(response);

			//Assert
			Assert.NotNull(actualFields);
			Assert.AreEqual(expectedFields.Length, actualFields.Count());
			CollectionAssert.AreEqual(expectedOrderedFieldEntries, actualFields);
		}


		[TestCase(new[] { "DisplayName" }, 0)]
		[TestCase(new[] { "a", "b" }, 0)]
		[TestCase(new[] { "b", "a", "c" }, 0)]
		[TestCase(new[] { "b", "A", "c" }, 0)]
		public void ItShouldGetAvailableFieldsForSavedSearch(string[] displayNames, int tmp)
		{
			//Arrange
			const int savedSearchArtifactId = 987;
			ExtendedSourceOptions extendedSourceOptions = CreateExtendedSourceOptions(ExportSettings.ExportType.SavedSearch, savedSearchArtifactId, 0, 0);
			FieldEntry[] expectedFields = CreateFieldsEntry(displayNames);
			IOrderedEnumerable<FieldEntry> expectedOrderedFieldEntries = expectedFields.OrderBy(x => x.DisplayName);

			_exportFieldsService.GetDefaultViewFields(_SOURCE_WORKSPACE_ARTIFACT_ID, savedSearchArtifactId,
				_TRANSFERRED_ARTIFACT_TYPE_ID, Arg.Any<bool>()).Returns(expectedFields);

			//Act
			HttpResponseMessage response = _instance.GetAvailableFields(extendedSourceOptions);
			IOrderedEnumerable<FieldEntry> actualFields = ExtractFieldEntryFromResponse(response);

			//Assert
			Assert.NotNull(actualFields);
			Assert.AreEqual(expectedFields.Length, actualFields.Count());
			CollectionAssert.AreEqual(expectedOrderedFieldEntries, actualFields);
		}

		[TestCase(new[] { "DisplayName" }, 0)]
		[TestCase(new[] { "a", "b" }, 0)]
		[TestCase(new[] { "b", "a", "c" }, 0)]
		[TestCase(new[] { "b", "A", "c" }, 0)]
		public void ItShouldGetAvailableFieldsForProductionSet(string[] displayNames, int tmp)
		{
			//Arrange
			const int productionId = 465432;
			ExtendedSourceOptions extendedSourceOptions = CreateExtendedSourceOptions(ExportSettings.ExportType.ProductionSet, 0, productionId, 0);
			FieldEntry[] expectedFields = CreateFieldsEntry(displayNames);
			IOrderedEnumerable<FieldEntry> expectedOrderedFieldEntries = expectedFields.OrderBy(x => x.DisplayName);

			_exportFieldsService.GetDefaultViewFields(_SOURCE_WORKSPACE_ARTIFACT_ID, productionId,
				_TRANSFERRED_ARTIFACT_TYPE_ID, Arg.Any<bool>()).Returns(expectedFields);

			//Act
			HttpResponseMessage response = _instance.GetAvailableFields(extendedSourceOptions);
			IOrderedEnumerable<FieldEntry> actualFields = ExtractFieldEntryFromResponse(response);

			//Assert
			Assert.NotNull(actualFields);
			Assert.AreEqual(expectedFields.Length, actualFields.Count());
			CollectionAssert.AreEqual(expectedOrderedFieldEntries, actualFields);
		}

		[TestCase(new[] { "DisplayName" }, ExportSettings.ExportType.Folder)]
		[TestCase(new[] { "a", "b" }, ExportSettings.ExportType.Folder)]
		[TestCase(new[] { "b", "a", "c" }, ExportSettings.ExportType.Folder)]
		[TestCase(new[] { "b", "A", "c" }, ExportSettings.ExportType.Folder)]
		[TestCase(new[] { "DisplayName" }, ExportSettings.ExportType.FolderAndSubfolders)]
		[TestCase(new[] { "a", "b" }, ExportSettings.ExportType.FolderAndSubfolders)]
		[TestCase(new[] { "b", "a", "c" }, ExportSettings.ExportType.FolderAndSubfolders)]
		[TestCase(new[] { "b", "A", "c" }, ExportSettings.ExportType.FolderAndSubfolders)]
		public void ItShouldGetAvailableFieldsForViewId(string[] displayNames, ExportSettings.ExportType exportType)
		{
			//Arrange
			const int viewId = 564912132;
			ExtendedSourceOptions extendedSourceOptions = CreateExtendedSourceOptions(exportType, 0, 0, viewId);
			FieldEntry[] expectedFields = CreateFieldsEntry(displayNames);
			IOrderedEnumerable<FieldEntry> expectedOrderedFieldEntries = expectedFields.OrderBy(x => x.DisplayName);

			_exportFieldsService.GetDefaultViewFields(_SOURCE_WORKSPACE_ARTIFACT_ID, viewId,
				_TRANSFERRED_ARTIFACT_TYPE_ID, Arg.Any<bool>()).Returns(expectedFields);

			//Act
			HttpResponseMessage response = _instance.GetAvailableFields(extendedSourceOptions);
			IOrderedEnumerable<FieldEntry> actualFields = ExtractFieldEntryFromResponse(response);

			//Assert
			Assert.NotNull(actualFields);
			Assert.AreEqual(expectedFields.Length, actualFields.Count());
			CollectionAssert.AreEqual(expectedOrderedFieldEntries, actualFields);
		}

		[Test]
		public void ItShouldThrowWhenGetAvailableFields()
		{
			//Arrange
			var data = new ExtendedSourceOptions()
			{
				TransferredArtifactTypeId = _TRANSFERRED_ARTIFACT_TYPE_ID,
				Options = JsonConvert.SerializeObject(new ExportUsingSavedSearchSettings() { SourceWorkspaceArtifactId = _SOURCE_WORKSPACE_ARTIFACT_ID })
			};

			//Act
			var exception = Assert.Throws<InvalidEnumArgumentException>(() => _instance.GetAvailableFields(data));
			
			//Assert
			Assert.IsNotNull(exception);
			Assert.AreEqual(ExportFieldsController.InvalidExportType, exception.Message);
		}

		[TestCase(new[] {"DisplayName"}, 0)]
		[TestCase(new[] {"a", "b"}, 0)]
		[TestCase(new[] {"b", "a", "c"}, 0)]
		[TestCase(new[] {"b", "A", "c"}, 0)]
		public void ItShouldGetExportableLongTextFields(string[] displayNames, int tmp)
		{
			//Arrange
			const int artifactTypeId = 97321;
			FieldEntry[] expectedFields = CreateFieldsEntry(displayNames);
			IOrderedEnumerable<FieldEntry> expectedOrderedFieldEntries = expectedFields.OrderBy(x => x.DisplayName);
			_exportFieldsService.GetAllExportableLongTextFields(_SOURCE_WORKSPACE_ARTIFACT_ID, artifactTypeId)
				.Returns(expectedFields);

			//Act
			HttpResponseMessage response = _instance.GetExportableLongTextFields(_SOURCE_WORKSPACE_ARTIFACT_ID, artifactTypeId);
			IOrderedEnumerable<FieldEntry> actualFields = ExtractFieldEntryFromResponse(response);

			//Assert
			Assert.NotNull(actualFields);
			Assert.AreEqual(expectedFields.Length, actualFields.Count());
			CollectionAssert.AreEqual(expectedOrderedFieldEntries, actualFields);
		}

		#region "Helpers"

		private static ExtendedSourceOptions CreateExtendedSourceOptions(ExportSettings.ExportType exportType, int savedSearchArtifactId, int productionId, int viewId)
		{
			var exportUsingSavedSearchSettings = new ExportUsingSavedSearchSettings()
			{
				SourceWorkspaceArtifactId = _SOURCE_WORKSPACE_ARTIFACT_ID,
				SavedSearchArtifactId = savedSearchArtifactId,
				ProductionId = productionId,
				ViewId = viewId,
				ExportType = exportType.ToString()
			};
			var data = new ExtendedSourceOptions()
			{
				TransferredArtifactTypeId = _TRANSFERRED_ARTIFACT_TYPE_ID,
				Options = JsonConvert.SerializeObject(exportUsingSavedSearchSettings)
			};

			return data;
		}

		private static FieldEntry[] CreateFieldsEntry(string[] displayNames)
		{
			return displayNames.Select((t, i) => new FieldEntry() {DisplayName = t, FieldIdentifier = i.ToString()}).ToArray();
		}

		private static IOrderedEnumerable<FieldEntry> ExtractFieldEntryFromResponse(HttpResponseMessage response)
		{
			var objectContent = response.Content as ObjectContent;
			var result = (IOrderedEnumerable<FieldEntry>)objectContent?.Value;
			return result;
		}

		#endregion
	}
}
