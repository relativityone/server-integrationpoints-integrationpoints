using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.System.Sanitizers
{
    internal class LongTextFieldSanitizerTests : SystemTest
    {
        private const string _CONTROL_NUMBER_WITH_LONG_TEXT_ABOVE_LIMIT = "long_text_above_100_signs";
        private const string _CONTROL_NUMBER_WITH_LONG_TEXT_BELOW_LIMIT = "long_text_below_100_signs";
        private const int _MAX_CHARS_FOR_LONG_TEXT_VALUES = 100;

        private readonly Guid _EXPORT_RUN_ID = Guid.NewGuid();
        private readonly Dataset _testData = Dataset.NativesAndExtractedText;

        private WorkspaceRef _workspace;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            _workspace = await Environment.CreateWorkspaceAsync().ConfigureAwait(false);

            ImportDataTableWrapper dataTableWrapper = DataTableFactory.CreateImportDataTable(
            _testData, extractedText: true, natives: false);

            var import = new ImportHelper(ServiceFactory);
            await import.ImportDataAsync(_workspace.ArtifactID, dataTableWrapper).ConfigureAwait(false);
        }

        [Test]
        public async Task SanitizeAsync_ShouldReturnValue_WhenLongTextIsBelowLimit()
        {
            // Arrange
            object value = await GetExtractedText(_CONTROL_NUMBER_WITH_LONG_TEXT_BELOW_LIMIT).ConfigureAwait(false);

            // Act
            var result = await RunTestCaseAsync(_CONTROL_NUMBER_WITH_LONG_TEXT_BELOW_LIMIT, value).ConfigureAwait(false);

            // Assert
            result.Should().Be(value);
        }

        [Test]
        public async Task SanitizeAsync_ShouldReturnStream_WhenLongTextIsAboveLimit()
        {
            // Arrange
            object value = await GetExtractedText(_CONTROL_NUMBER_WITH_LONG_TEXT_ABOVE_LIMIT).ConfigureAwait(false);

            // Act
            var result = await RunTestCaseAsync(_CONTROL_NUMBER_WITH_LONG_TEXT_ABOVE_LIMIT, value).ConfigureAwait(false);

            // Assert
            result.Should().BeAssignableTo<Stream>();
        }

        private async Task<object> RunTestCaseAsync(string identifier, object value)
        {
            IExportFieldSanitizer sut = GetSut();

            return await sut.SanitizeAsync(
                _workspace.ArtifactID,
                "Control Number",
                identifier,
                "Extracted Text",
                value);
        }

        private IExportFieldSanitizer GetSut()
        {
            IContainer container = ContainerHelper.Create(
                new Common.ConfigurationStub()
                {
                    SourceWorkspaceArtifactId = _workspace.ArtifactID,
                    ExportRunId = _EXPORT_RUN_ID
                });

            return container.Resolve<IEnumerable<IExportFieldSanitizer>>().Single(x => x is LongTextFieldSanitizer);
        }

        private async Task<object> GetExtractedText(string controlNumber)
        {
            using (IObjectManager objectManager = ServiceFactory.CreateProxy<IObjectManager>())
            {
                var request = new QueryRequest()
                {
                    ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Document },
                    Condition = $"'Control Number' == '{controlNumber}'",
                    Fields = new[]
                    {
                        new FieldRef { Name = "Extracted Text" }
                    },
                    LongTextBehavior = LongTextBehavior.Tokenized,
                    MaxCharactersForLongTextValues = _MAX_CHARS_FOR_LONG_TEXT_VALUES
                };

                var result = await objectManager.QuerySlimAsync(_workspace.ArtifactID, request, 0, 1).ConfigureAwait(false);

                return result.Objects.Single().Values[0];
            }
        }
    }
}
