﻿using System;
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
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit.Executors
{
	[TestFixture]
	public sealed class JobHistoryNameQueryTests
	{
		private Mock<IObjectManager> _objectManager;

		private JobHistoryNameQuery _sut;

		[SetUp]
		public void SetUp()
		{
			_objectManager = new Mock<IObjectManager>();
			var serviceFactory = new Mock<ISourceServiceFactoryForUser>();
			serviceFactory.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);

			_sut = new JobHistoryNameQuery(serviceFactory.Object, new EmptyLogger());
		}

		[Test]
		public async Task ItShouldReturnJobName()
		{
			const string jobName = "job";
			QueryResult queryResult = new QueryResult()
			{
				Objects = new List<RelativityObject>()
				{
					new RelativityObject()
					{
						Name = jobName
					}
				}
			};
			_objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(), CancellationToken.None, It.IsAny<IProgress<ProgressReport>>()))
				.ReturnsAsync(queryResult);

			// act
			string actualJobName = await _sut.GetJobNameAsync(0, 0, CancellationToken.None).ConfigureAwait(false);

			// assert
			Assert.AreEqual(jobName, actualJobName);
		}

		[Test]
		public void ItShouldRethrowExceptionWhenObjectManagerQueryFails()
		{
			_objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(), CancellationToken.None, It.IsAny<IProgress<ProgressReport>>()))
				.Throws<InvalidOperationException>();

			// act
			Func<Task> action = async () => await _sut.GetJobNameAsync(0, 0, CancellationToken.None).ConfigureAwait(false);

			// assert
			action.Should().Throw<InvalidOperationException>();
		}

		[Test]
		public void ItShouldThrowSyncExceptionWhenObjectManagerReturnsNoResults()
		{
			QueryResult queryResult = new QueryResult()
			{
				Objects = Enumerable.Empty<RelativityObject>().ToList()
			};
			_objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(), CancellationToken.None, It.IsAny<IProgress<ProgressReport>>()))
				.ReturnsAsync(queryResult);

			// act
			Func<Task> action = async () => await _sut.GetJobNameAsync(0, 0, CancellationToken.None).ConfigureAwait(false);

			// assert
			action.Should().Throw<SyncException>();
		}
	}
}