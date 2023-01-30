using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Tests.System.ExecutorTests.TestsSetup;
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
        private string _workspaceFileSharePath;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            _workspace = await Environment.CreateWorkspaceAsync().ConfigureAwait(false);

            ImportDataTableWrapper dataTableWrapper = DataTableFactory.CreateImportDataTable(
            _testData, extractedText: true, natives: false);

            var import = new ImportHelper(ServiceFactory);
            await import.ImportDataAsync(_workspace.ArtifactID, dataTableWrapper).ConfigureAwait(false);
        }

        [SetUp]
        public void SetUp()
        {
            _workspaceFileSharePath = Path.Combine(Path.GetTempPath(), _workspace.Name);

            Directory.CreateDirectory(_workspaceFileSharePath);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_workspaceFileSharePath))
            {
                Directory.Delete(_workspaceFileSharePath, true);
            }
        }

        [Test]
        public async Task SanitizeAsync_ShouldReturnValue_WhenLongTextIsBelowLimit()
        {
            // Arrange
            object value = await GetExtractedText(_CONTROL_NUMBER_WITH_LONG_TEXT_BELOW_LIMIT).ConfigureAwait(false);

            // Act
            var result = await RunTestCaseAsync(_CONTROL_NUMBER_WITH_LONG_TEXT_BELOW_LIMIT, value, false).ConfigureAwait(false);

            // Assert
            result.Should().Be(value);
        }

        [Test]
        public async Task SanitizeAsync_ShouldReturnStream_WhenLongTextIsAboveLimit()
        {
            // Arrange
            object value = await GetExtractedText(_CONTROL_NUMBER_WITH_LONG_TEXT_ABOVE_LIMIT).ConfigureAwait(false);

            // Act
            var result = await RunTestCaseAsync(_CONTROL_NUMBER_WITH_LONG_TEXT_ABOVE_LIMIT, value, false).ConfigureAwait(false);

            // Assert
            result.Should().BeAssignableTo<Stream>();
        }

        [Test]
        public async Task SanitizeAsync_ShouldReturnRelativePathWithValue_WhenLongTextBelowLimitIsReadWithNewImport()
        {
            // Arrange
            object value = await GetExtractedText(_CONTROL_NUMBER_WITH_LONG_TEXT_BELOW_LIMIT).ConfigureAwait(false);

            string expectedText = value.ToString();

            // Act
            var result = await RunTestCaseAsync(_CONTROL_NUMBER_WITH_LONG_TEXT_BELOW_LIMIT, value, true).ConfigureAwait(false);

            // Assert
            AssertLongTextGeneratedFile(result, expectedText);
        }

        [Test]
        public async Task SanitizeAsync_ShouldReturnRelativePathWithValue_WhenLongTextAboveLimitIsReadWithNewImport()
        {
            // Arrange
            object value = await GetExtractedText(_CONTROL_NUMBER_WITH_LONG_TEXT_ABOVE_LIMIT).ConfigureAwait(false);

            string expectedText = File.ReadAllText(
                Path.Combine(_testData.FolderPath, "TEXT", $"{_CONTROL_NUMBER_WITH_LONG_TEXT_ABOVE_LIMIT}.txt"));

            // Act
            var result = await RunTestCaseAsync(_CONTROL_NUMBER_WITH_LONG_TEXT_ABOVE_LIMIT, value, true).ConfigureAwait(false);

            // Assert
            AssertLongTextGeneratedFile(result, expectedText);
        }

        private async Task<object> RunTestCaseAsync(string identifier, object value, bool useNewImport)
        {
            IExportFieldSanitizer sut = GetSut(useNewImport);

            return await sut.SanitizeAsync(
                _workspace.ArtifactID,
                "Control Number",
                identifier,
                "Extracted Text",
                value);
        }

        private IExportFieldSanitizer GetSut(bool shouldUseNewImport)
        {
            Mock<IIAPIv2RunChecker> newImportCheckMock = new Mock<IIAPIv2RunChecker>();
            newImportCheckMock.Setup(x => x.ShouldBeUsed()).Returns(shouldUseNewImport);

            IFileShareService fileShareMock = new FileShareServiceMock(_workspaceFileSharePath);

            IContainer container = ContainerHelper.Create(
                new Common.ConfigurationStub()
                {
                    SourceWorkspaceArtifactId = _workspace.ArtifactID,
                    ExportRunId = _EXPORT_RUN_ID
                },
                mockActions: b =>
                {
                    b.RegisterInstance(newImportCheckMock.Object);
                    b.RegisterInstance(fileShareMock);
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

        private void AssertLongTextGeneratedFile(object result, string expectedText)
        {
            string longTextPath = result.ToString();

            Path.IsPathRooted(longTextPath).Should().BeFalse();

            string fullPath = Path.Combine(GetSyncJobPath(), longTextPath);

            File.ReadAllText(fullPath).Should().Be(expectedText);
        }

        private string GetSyncJobPath()
        {
            return Path.Combine(_workspaceFileSharePath, "Sync", _EXPORT_RUN_ID.ToString());
        }
    }
}
