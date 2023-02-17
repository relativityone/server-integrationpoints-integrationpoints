using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.Web.DataStructures;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using Relativity.IntegrationPoints.Contracts.Models;
using CategoryAttribute = NUnit.Framework.CategoryAttribute;
using ExportSettings = kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportSettings;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API
{
    [TestFixture, Category("Unit")]
    public class ExportFieldsControllerTests : TestBase
    {
        private ExportFieldsController _instance;
        private IExportFieldsService _exportFieldsService;
        private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 123951;
        private const int _TRANSFERRED_ARTIFACT_TYPE_ID = 1234;
        private const int _VIEW_ID = 564912132;
        private const int _PRODUCTION_ID = 465432;
        private const int _SAVED_SEARCH_ARTIFACT_ID = 987;
        private static IEnumerable<IEnumerable<string>> DisplayNamesTestData() => new[]
        {
            new[] {"DisplayName" },
            new[] {"a", "b" },
            new[] {"b", "a", "c" },
            new[] {"b", "A", "c"}
        };

        private static IEnumerable<ExportSettings.ExportType> ExportTypesTestData() => new[]
        {
            ExportSettings.ExportType.Folder,
            ExportSettings.ExportType.FolderAndSubfolders
        };

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

        [TestCaseSource(nameof(DisplayNamesTestData))]
        public void ItShouldGetExportableFields(string[] displayNames)
        {
            // Arrange
            var data = new ExtendedSourceOptions()
            {
                TransferredArtifactTypeId = _TRANSFERRED_ARTIFACT_TYPE_ID,
                Options = JsonConvert.SerializeObject(new ExportUsingSavedSearchSettings() {SourceWorkspaceArtifactId = _SOURCE_WORKSPACE_ARTIFACT_ID })
            };
            FieldEntry[] expectedFields = CreateFieldsEntry(displayNames);
            IOrderedEnumerable<FieldEntry> expectedOrderedFieldEntries = expectedFields.OrderBy(x => x.DisplayName);

            _exportFieldsService.GetAllExportableFields(_SOURCE_WORKSPACE_ARTIFACT_ID, _TRANSFERRED_ARTIFACT_TYPE_ID).Returns(expectedFields);

            // Act
            HttpResponseMessage response = _instance.GetExportableFields(data);
            IOrderedEnumerable<FieldEntry> actualFields = ExtractFieldEntryFromResponse(response);

            // Assert
            Assert.NotNull(actualFields);
            Assert.AreEqual(expectedFields.Length, actualFields.Count());
            CollectionAssert.AreEqual(expectedOrderedFieldEntries, actualFields);
        }

        [TestCaseSource(nameof(DisplayNamesTestData))]
        public void ItShouldGetAvailableFieldsForSavedSearch(string[] displayNames)
        {
            // Arrange
            ExtendedSourceOptions extendedSourceOptions = CreateExtendedSourceOptions(ExportSettings.ExportType.SavedSearch);
            FieldEntry[] expectedFields = CreateFieldsEntry(displayNames);
            IOrderedEnumerable<FieldEntry> expectedOrderedFieldEntries = expectedFields.OrderBy(x => x.DisplayName);

            _exportFieldsService.GetDefaultViewFields(_SOURCE_WORKSPACE_ARTIFACT_ID, _SAVED_SEARCH_ARTIFACT_ID,
                _TRANSFERRED_ARTIFACT_TYPE_ID, Arg.Any<bool>()).Returns(expectedFields);

            // Act
            HttpResponseMessage response = _instance.GetAvailableFields(extendedSourceOptions);
            IOrderedEnumerable<FieldEntry> actualFields = ExtractFieldEntryFromResponse(response);

            // Assert
            Assert.NotNull(actualFields);
            Assert.AreEqual(expectedFields.Length, actualFields.Count());
            CollectionAssert.AreEqual(expectedOrderedFieldEntries, actualFields);
        }

        [TestCaseSource(nameof(DisplayNamesTestData))]
        public void ItShouldGetAvailableFieldsForProductionSet(string[] displayNames)
        {
            // Arrange
            ExtendedSourceOptions extendedSourceOptions = CreateExtendedSourceOptions(ExportSettings.ExportType.ProductionSet);
            FieldEntry[] expectedFields = CreateFieldsEntry(displayNames);
            IOrderedEnumerable<FieldEntry> expectedOrderedFieldEntries = expectedFields.OrderBy(x => x.DisplayName);

            _exportFieldsService.GetDefaultViewFields(_SOURCE_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ID,
                _TRANSFERRED_ARTIFACT_TYPE_ID, Arg.Any<bool>()).Returns(expectedFields);

            // Act
            HttpResponseMessage response = _instance.GetAvailableFields(extendedSourceOptions);
            IOrderedEnumerable<FieldEntry> actualFields = ExtractFieldEntryFromResponse(response);

            // Assert
            Assert.NotNull(actualFields);
            Assert.AreEqual(expectedFields.Length, actualFields.Count());
            CollectionAssert.AreEqual(expectedOrderedFieldEntries, actualFields);
        }

        [Test, Sequential]
        public void ItShouldGetAvailableFieldsForViewId([ValueSource(nameof(DisplayNamesTestData))]string[] displayNames,
            [ValueSource(nameof(ExportTypesTestData))] ExportSettings.ExportType exportType)
        {
            // Arrange
            ExtendedSourceOptions extendedSourceOptions = CreateExtendedSourceOptions(exportType);
            FieldEntry[] expectedFields = CreateFieldsEntry(displayNames);
            IOrderedEnumerable<FieldEntry> expectedOrderedFieldEntries = expectedFields.OrderBy(x => x.DisplayName);

            _exportFieldsService.GetDefaultViewFields(_SOURCE_WORKSPACE_ARTIFACT_ID, _VIEW_ID,
                _TRANSFERRED_ARTIFACT_TYPE_ID, Arg.Any<bool>()).Returns(expectedFields);

            // Act
            HttpResponseMessage response = _instance.GetAvailableFields(extendedSourceOptions);
            IOrderedEnumerable<FieldEntry> actualFields = ExtractFieldEntryFromResponse(response);

            // Assert
            Assert.NotNull(actualFields);
            Assert.AreEqual(expectedFields.Length, actualFields.Count());
            CollectionAssert.AreEqual(expectedOrderedFieldEntries, actualFields);
        }

        [Test]
        public void ItShouldThrowWhenGetAvailableFields()
        {
            // Arrange
            var data = new ExtendedSourceOptions()
            {
                TransferredArtifactTypeId = _TRANSFERRED_ARTIFACT_TYPE_ID,
                Options = JsonConvert.SerializeObject(new ExportUsingSavedSearchSettings() { SourceWorkspaceArtifactId = _SOURCE_WORKSPACE_ARTIFACT_ID })
            };

            // Act
            var exception = Assert.Throws<InvalidEnumArgumentException>(() => _instance.GetAvailableFields(data));

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(Constants.INVALID_EXPORT_TYPE_ERROR, exception.Message);
        }

        [TestCaseSource(nameof(DisplayNamesTestData))]
        public void ItShouldGetExportableLongTextFields(string[] displayNames)
        {
            // Arrange
            const int artifactTypeId = 97321;
            FieldEntry[] expectedFields = CreateFieldsEntry(displayNames);
            IOrderedEnumerable<FieldEntry> expectedOrderedFieldEntries = expectedFields.OrderBy(x => x.DisplayName);
            _exportFieldsService.GetAllExportableLongTextFields(_SOURCE_WORKSPACE_ARTIFACT_ID, artifactTypeId)
                .Returns(expectedFields);

            // Act
            HttpResponseMessage response = _instance.GetExportableLongTextFields(_SOURCE_WORKSPACE_ARTIFACT_ID, artifactTypeId);
            IOrderedEnumerable<FieldEntry> actualFields = ExtractFieldEntryFromResponse(response);

            // Assert
            Assert.NotNull(actualFields);
            Assert.AreEqual(expectedFields.Length, actualFields.Count());
            CollectionAssert.AreEqual(expectedOrderedFieldEntries, actualFields);
        }

        #region "Helpers"

        private static ExtendedSourceOptions CreateExtendedSourceOptions(ExportSettings.ExportType exportType)
        {
            var exportUsingSavedSearchSettings = new ExportUsingSavedSearchSettings()
            {
                SourceWorkspaceArtifactId = _SOURCE_WORKSPACE_ARTIFACT_ID,
                SavedSearchArtifactId = _SAVED_SEARCH_ARTIFACT_ID,
                ProductionId = _PRODUCTION_ID,
                ViewId = _VIEW_ID,
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
