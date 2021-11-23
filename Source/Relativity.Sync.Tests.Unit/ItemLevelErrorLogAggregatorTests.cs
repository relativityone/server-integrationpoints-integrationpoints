using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Logging;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit
{
    [TestFixture]
    public class ItemLevelErrorLogAggregatorTests
    {
        private Mock<ISyncLog> _loggerMock;
        private ItemLevelErrorLogAggregator _sut;

        private const string IdentifierReplacement = "[identifier]";
        private const string SampleErrorMessage1 = "Sample error message 1";
        private const string SampleErrorMessage2 = "Sample error message 2";

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ISyncLog>();
            _sut = new ItemLevelErrorLogAggregator(_loggerMock.Object);
        }

        [Test]
        public async Task LogAllItemLevelErrors_ShouldLogAllAddedErrors()
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
                    ("ABC", "IAPI  - An item with identifier ABC already exists in the workspace"),
                    ("QWERTY", "IAPI  - An item with identifier QWERTY already exists in the workspace"),
                    ("Kapitan Bomba", "IAPI  - An item with identifier Kapitan Bomba already exists in the workspace")
                },
                "IAPI  - An item with identifier [identifier] already exists in the workspace"
            )
            {
                TestName = "IAPI  - An item with identifier [identifier] already exists in the workspace"
            };
            
            yield return new TestCaseData(
                new[]
                {
                    ("ABC", "IAPI  - 20.006. Failed to copy source field into destination field due to missing child object. Review the following destination field(s): Control Number, Extracted Text"),
                    ("QWERTY", "IAPI  - 20.006. Failed to copy source field into destination field due to missing child object. Review the following destination field(s): Some Field"),
                },
                "IAPI  - 20.006. Failed to copy source field into destination field due to missing child object"
            )
            {
                TestName = "IAPI  - 20.006. Failed to copy source field into destination field due to missing child object. Review the following destination field(s):"
            };
        }
    }
}