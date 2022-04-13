using Relativity.API;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Logging;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Logging
{
    [TestFixture]
    public class ItemLevelErrorLogAggregatorTests
    {
        private Mock<IAPILog> _loggerMock;
        private ItemLevelErrorLogAggregator _sut;

        private const string SampleErrorMessage1 = "Sample error message 1";
        private const string SampleErrorMessage2 = "Sample error message 2";

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<IAPILog>();
            _sut = new ItemLevelErrorLogAggregator(_loggerMock.Object);
        }

        [Test]
        public async Task LogAllItemLevelErrorsAsync_ShouldLogAllAddedErrors()
        {
            // Arrange
            foreach ((ItemLevelError error, int i) in GetErrors().Select((x, i) => (x, i)))
            {
                _sut.AddItemLevelError(error, i);
            }

            // Act
            await _sut.LogAllItemLevelErrorsAsync().ConfigureAwait(false);

            // Assert
            _loggerMock.Verify(x => x.LogWarning("Item level error occured: {message} -> [{items}]",
                It.Is((string s) => s == SampleErrorMessage1)
                , It.Is((string ids) => ids == "0, 2, 3")));

            _loggerMock.Verify(x => x.LogWarning("Item level error occured: {message} -> [{items}]",
                It.Is((string s) => s == SampleErrorMessage2)
                , It.Is((string ids) => ids == "1, 4")));
        }

        [Test]
        public async Task LogAllItemLevelErrorsAsync_ShouldLogTotalCountOfErrors()
        {
            // Arrange
            foreach ((ItemLevelError error, int i) in GetErrors().Select((x, i) => (x, i)))
            {
                _sut.AddItemLevelError(error, i);
            }

            // Act
            await _sut.LogAllItemLevelErrorsAsync().ConfigureAwait(false);

            // Assert
            _loggerMock.Verify(x => x.LogWarning("Total count of item level errors in batch: {count}", 5), Times.Once);
        }

        [TestCaseSource(nameof(KnownItemLevelErrors))]
        public async Task ShouldGroupAllKnowItemLevelErrors((string identifier, string error)[] errors,
            string expectedCleanedUpMessage)
        {
            // Arrange
            foreach ((ItemLevelError error, int i) in errors
                .Select((x, i) =>
                    (new ItemLevelError(x.identifier, x.error), i))
            )
            {
                _sut.AddItemLevelError(error, i);
            }

            // Act
            await _sut.LogAllItemLevelErrorsAsync().ConfigureAwait(false);

            // Assert
            _loggerMock.Verify(x => x.LogWarning("Item level error occured: {message} -> [{items}]",
                It.Is((string s) => s == expectedCleanedUpMessage)
                , It.Is((string ids) => ids == string.Join(", ", Enumerable.Range(0, errors.Length)))));
        }

        private IEnumerable<ItemLevelError> GetErrors()
        {
            yield return new ItemLevelError("Adler Sieben", SampleErrorMessage1);
            yield return new ItemLevelError("Adler Sieben 2", SampleErrorMessage2);
            yield return new ItemLevelError("Adler Sieben 3", SampleErrorMessage1);
            yield return new ItemLevelError("RIP", SampleErrorMessage1);
            yield return new ItemLevelError("RIP 2", SampleErrorMessage2);
        }

        static IEnumerable<TestCaseData> KnownItemLevelErrors()
        {
            yield return new TestCaseData(
                new[]
                {
                    ("ABC",
                        "IAPI  - 20.006. Failed to copy source field into destination field due to missing child object. Review the following destination field(s): Control Number, Extracted Text"),
                    ("QWERTY",
                        "IAPI  - 20.006. Failed to copy source field into destination field due to missing child object. Review the following destination field(s): Some Field"),
                },
                "IAPI  - 20.006. Failed to copy source field into destination field due to missing child object"
            )
            {
                TestName =
                    "IAPI  - 20.006. Failed to copy source field into destination field due to missing child object. Review the following destination field(s):"
            };

            yield return new TestCaseData(
                new[]
                {
                    ("ABC", "IAPI  - An item with identifier ABC already exists in the workspace"),
                    ("ABC",
                        "IAPI  - An item with identifier ABC already exists in the workspace* - Field * Error: Cannot create * because a file or directory with the same name already exists."),
                    ("ABC",
                        "IAPI  - An item with identifier ABC already exists in the workspace* - Field * Error: Incorrect function.*"),
                    ("ABC",
                        "IAPI  - An item with identifier ABC already exists in the workspace* - A non unique associated object is specified for this new object"),
                },
                "IAPI  - An item with identifier {0} already exists in the workspace"
            )
            {
                TestName = "IAPI  - An item with identifier {0} already exists in the workspace"
            };

            yield return new TestCaseData(
                new[]
                {
                    ("ABC", "IAPI  - A non unique associated object is specified for this new object"),
                    ("DEF",
                        "IAPI  - A non unique associated object is specified for this new object* - Field * Error: Incorrect function.*")
                },
                "IAPI  - A non unique associated object is specified for this new object"
            )
            {
                TestName = "IAPI  - A non unique associated object is specified for this new object"
            };

            yield return new TestCaseData(
                new[]
                {
                    ("ABC",
                        "IAPI Error in line 10, column 6. The input value from the * source field has a length of * character(s). This exceeds the limit for the * destination field, which is currently set to * character(s)."),
                    ("ABC",
                        "IAPI Error in line *, column *. Object identifier for field * references an identifier that is not unique.")
                },
                "IAPI Error in line *, column *."
            ) { TestName = "IAPI Error in line *, column *." };

            yield return new TestCaseData(
                new[]
                {
                    ("ABC", "IAPI  - Field Control Number Error: Incorrect function."),
                    ("ABC",
                        "IAPI  - Field Control Number Error: Cannot create * because a file or directory with the same name already exists."),
                    ("ABC", @"IAPI  - Field Control Number Error: Could not find file \\path\to\file."),
                    ("ABC", "IAPI  - Field Control Number Error: Insufficient system resources exist to complete the requested service."),
                },
                "IAPI Error in line *, column *."
            ) { TestName = "IAPI  - Field * Error" };

            yield return new TestCaseData(
                new[]
                {
                    ("ABC", "IAPI  - One of the files specified for this document does not exist" ),
                },
                "IAPI  - One of the files specified for this document does not exist"
            ) { TestName = "IAPI  - One of the files specified for this document does not exist" };
        }
    }
}
