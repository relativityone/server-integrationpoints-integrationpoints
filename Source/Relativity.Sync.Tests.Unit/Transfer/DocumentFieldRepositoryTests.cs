using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
	[TestFixture]
	public class DocumentFieldRepositoryTests
	{
		private int _sourceWorkspaceArtifactId = 1234;
		private Mock<ISourceServiceFactoryForUser> _serviceFactory;
		private DocumentFieldRepository _instance;
		private Mock<IObjectManager> _objectManager;

		[SetUp]
		public void SetUp()
		{
			_objectManager = new Mock<IObjectManager>();

			_serviceFactory = new Mock<ISourceServiceFactoryForUser>();
			_serviceFactory.Setup(f => f.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);
			_instance = new DocumentFieldRepository(_serviceFactory.Object);
		}

		[Test]
		public async Task ItShouldReturnFieldWithMappedRelativityType()
		{
			// Arrange
			string field1Name = "Field 1";
			string field1RelativityDataTypeName = "Date";
			RelativityDataType field1RelativityDataType = RelativityDataType.Date;

			string field2Name = "Field 2";
			string field2RelativityDataTypeName = "Long Text";
			RelativityDataType field2RelativityDataType = RelativityDataType.LongText;

			List<string> fieldNames = new List<string> {field1Name, field2Name};
			
			List<RelativityObjectSlim> returnObjects = new List<RelativityObjectSlim>
			{
				new RelativityObjectSlim {Values = new List<object> {field1Name, field1RelativityDataTypeName}},
				new RelativityObjectSlim {Values = new List<object> {field2Name, field2RelativityDataTypeName}}
			};

			QueryResultSlim queryResult = new QueryResultSlim {Objects = returnObjects};

			const int start = 0;
			_objectManager.Setup(om => om.QuerySlimAsync(_sourceWorkspaceArtifactId, It.IsAny<QueryRequest>(), start, fieldNames.Count, CancellationToken.None)).ReturnsAsync(queryResult);

			// Act
			Dictionary<string, RelativityDataType> result = await _instance
				.GetRelativityDataTypesForFieldsByFieldName(_sourceWorkspaceArtifactId, fieldNames).ConfigureAwait(false);

			// Assert
			result.Count.Should().Be(fieldNames.Count);
			result.Should().ContainKey(field1Name).WhichValue.Should().Be(field1RelativityDataType);
			result.Should().ContainKey(field2Name).WhichValue.Should().Be(field2RelativityDataType);
		}

		[Test]
		public async Task ItShouldNotCallObjectManagerWhenFieldListIsEmpty()
		{
			// Arrange
			List<string> emptyFieldNamesList = new List<string>();

			// Act
			Func<Task<Dictionary<string, RelativityDataType>>> action = () =>
				_instance.GetRelativityDataTypesForFieldsByFieldName(_sourceWorkspaceArtifactId, emptyFieldNamesList);

			// Assert
			await action.Should().NotThrowAsync().ConfigureAwait(false);
			_objectManager.Verify(om => om.QuerySlimAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(), CancellationToken.None), Times.Never);
		}
	}
}
