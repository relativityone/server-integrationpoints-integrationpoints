using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.DataContracts.DTOs;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit.Executors.Validation
{
	[TestFixture]
	public class FolderStructureBehaviorValidatorTests
	{
		private CancellationToken _cancellationToken;

		private Mock<ISourceServiceFactoryForUser> _sourceServiceFactoryForUser;
		private Mock<IObjectManager> _objectManager;
		private Mock<IValidationConfiguration> _validationConfiguration;

		private FolderStructureBehaviorValidator _instance;

		private const int _TEST_FOLDER_ARTIFACT_ID = 101234;
		private const int _TEST_WORKSPACE_ARTIFACT_ID = 101202;
		private const string _EXPECTED_QUERY_FIELD_TYPE = "Field Type";

		[SetUp]
		public void SetUp()
		{
			_cancellationToken = CancellationToken.None;

			_sourceServiceFactoryForUser = new Mock<ISourceServiceFactoryForUser>();
			_objectManager = new Mock<IObjectManager>();
			_validationConfiguration = new Mock<IValidationConfiguration>();

			_validationConfiguration.SetupGet(x => x.FolderPathSourceFieldArtifactId).Returns(_TEST_FOLDER_ARTIFACT_ID);
			_validationConfiguration.SetupGet(x => x.SourceWorkspaceArtifactId).Returns(_TEST_WORKSPACE_ARTIFACT_ID);
			_validationConfiguration.SetupGet(x => x.DestinationFolderStructureBehavior).Returns(DestinationFolderStructureBehavior.ReadFromField);

			_instance = new FolderStructureBehaviorValidator(_sourceServiceFactoryForUser.Object, new EmptyLogger());
		}

		[Test]
		public async Task ValidateAsyncGoldFlowTest()
		{
			// Arrange
			_sourceServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object).Verifiable();

			QueryResult queryResult = BuildQueryResult("Long Text");
			_objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<IProgress<ProgressReport>>()))
				.ReturnsAsync(queryResult);

			// Act
			ValidationResult actualResult = await _instance.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			Assert.IsTrue(actualResult.IsValid);

			VerifyObjectManagerQueryRequest();

			Mock.VerifyAll(_sourceServiceFactoryForUser, _objectManager);
			_objectManager.Verify(x => x.Dispose(), Times.Once);
		}

		[Test]
		public async Task ValidateAsyncInvalidFieldTypeResultTest()
		{
			// Arrange
			_sourceServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object).Verifiable();

			QueryResult queryResult = BuildQueryResult("Invalid Field Type Here");
			_objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<IProgress<ProgressReport>>()))
				.ReturnsAsync(queryResult);

			// Act
			ValidationResult actualResult = await _instance.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			Assert.IsFalse(actualResult.IsValid);
			Assert.IsNotEmpty(actualResult.Messages);
			Assert.AreEqual(1, actualResult.Messages.Count());

			VerifyObjectManagerQueryRequest();

			Mock.VerifyAll(_sourceServiceFactoryForUser, _objectManager);
			_objectManager.Verify(x => x.Dispose(), Times.Once);
		}

		[Test]
		public async Task ValidateAsyncNoQueryResultsTest()
		{
			// Arrange
			_sourceServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object).Verifiable();

			QueryResult queryResult = new QueryResult { Objects = new List<RelativityObject>() };
			_objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<IProgress<ProgressReport>>()))
				.ReturnsAsync(queryResult);

			// Act
			ValidationResult actualResult = await _instance.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			Assert.IsFalse(actualResult.IsValid);
			Assert.IsNotEmpty(actualResult.Messages);
			Assert.AreEqual(1, actualResult.Messages.Count());

			VerifyObjectManagerQueryRequest();

			Mock.VerifyAll(_sourceServiceFactoryForUser, _objectManager);
			_objectManager.Verify(x => x.Dispose(), Times.Once);
		}

		[Test]
		public async Task ValidateAsyncCreateProxyThrowsExceptionTest()
		{
			// Arrange
			_sourceServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).Throws<InvalidOperationException>().Verifiable();

			// Act
			ValidationResult actualResult = await _instance.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			Assert.IsFalse(actualResult.IsValid);
			Assert.IsNotEmpty(actualResult.Messages);
			Assert.AreEqual(1, actualResult.Messages.Count());

			Mock.VerifyAll(_sourceServiceFactoryForUser, _objectManager);
		}

		[Test]
		public async Task ValidateAsyncQueryAsyncThrowsExceptionTest()
		{
			// Arrange
			_sourceServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object).Verifiable();

			_objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<IProgress<ProgressReport>>()))
				.Throws<InvalidOperationException>();

			// Act
			ValidationResult actualResult = await _instance.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			Assert.IsFalse(actualResult.IsValid);
			Assert.IsNotEmpty(actualResult.Messages);
			Assert.AreEqual(1, actualResult.Messages.Count());

			Mock.VerifyAll(_sourceServiceFactoryForUser, _objectManager);
			_objectManager.Verify(x => x.Dispose(), Times.Once);
		}

		[Test]
		public async Task ItShouldNotValidateWhenBehaviorIsSetToNone()
		{
			_validationConfiguration.SetupGet(x => x.DestinationFolderStructureBehavior).Returns(DestinationFolderStructureBehavior.None);

			// Act
			ValidationResult actualResult = await _instance.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			_sourceServiceFactoryForUser.Verify(x => x.CreateProxyAsync<IObjectManager>(), Times.Never);
			actualResult.IsValid.Should().Be(true);
		}

		[Test]
		public async Task ItShouldNotValidateWhenBehaviorIsSetToRetainFromSourceWorkspace()
		{
			_validationConfiguration.SetupGet(x => x.DestinationFolderStructureBehavior).Returns(DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure);

			// Act
			ValidationResult actualResult = await _instance.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			_sourceServiceFactoryForUser.Verify(x => x.CreateProxyAsync<IObjectManager>(), Times.Never);
			actualResult.IsValid.Should().Be(true);
		}

		private QueryResult BuildQueryResult(string testFieldValue)
		{
			var queryResult = new QueryResult
			{
				Objects = new List<RelativityObject>
				{
					new RelativityObject
					{
						FieldValues = new List<FieldValuePair>
						{
							new FieldValuePair
							{
								Field = new Field
								{
									Name = _EXPECTED_QUERY_FIELD_TYPE
								},
								Value = testFieldValue
							}
						}
					}
				}
			};
			return queryResult;
		}

		private void VerifyObjectManagerQueryRequest()
		{
			string expectedQueryCondition = $"(('ArtifactID' == {_TEST_FOLDER_ARTIFACT_ID}))";

			_objectManager.Verify(x => x.QueryAsync(It.Is<int>(y => y == _TEST_WORKSPACE_ARTIFACT_ID),
					It.Is<QueryRequest>(y => y.ObjectType.Name == "Field" && y.Condition == expectedQueryCondition && y.Fields.First().Name == _EXPECTED_QUERY_FIELD_TYPE),
					It.Is<int>(y => y == 0), It.Is<int>(y => y == 1), It.Is<CancellationToken>(y => y == _cancellationToken), It.IsAny<IProgress<ProgressReport>>()));
		}
	}
}