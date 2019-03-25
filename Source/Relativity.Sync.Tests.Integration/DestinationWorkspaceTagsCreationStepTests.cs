using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.DataContracts.DTOs;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Tests.Integration.Stubs;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal sealed class DestinationWorkspaceTagsCreationStepTests : FailingStepsBase<IDestinationWorkspaceTagsCreationConfiguration>
	{
		private IExecutor<IDestinationWorkspaceTagsCreationConfiguration> _executor;
		private Mock<IObjectManager> _objectManagerMock;

		private static readonly Guid SourceCaseTagObjectTypeGuid = new Guid("7E03308C-0B58-48CB-AFA4-BB718C3F5CAC");
		private static readonly Guid SourceJobTagObjectType = new Guid("6f4dd346-d398-4e76-8174-f0cd8236cbe7");

		protected override void AssertExecutedSteps(List<Type> executorTypes)
		{
			executorTypes.Should().Contain(x => x == typeof(ISourceWorkspaceTagsCreationConfiguration));
			executorTypes.Should().Contain(x => x == typeof(IDataDestinationInitializationConfiguration));
		}

		protected override int ExpectedNumberOfExecutedSteps()
		{
			// validation, permissions, object types, snapshot, source workspace tags, data destination init, notification
			const int expectedNumberOfExecutedSteps = 7;
			return expectedNumberOfExecutedSteps;
		}

		[SetUp]
		public void MySetUp()
		{
			ContainerBuilder containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
			IntegrationTestsContainerBuilder.MockStepsExcept<IDestinationWorkspaceTagsCreationConfiguration>(containerBuilder);

			_objectManagerMock = new Mock<IObjectManager>();
			var serviceFactoryMock = new Mock<IDestinationServiceFactoryForUser>();
			var serviceFactoryMock2 = new Mock<ISourceServiceFactoryForUser>();
			serviceFactoryMock.Setup(x => x.CreateProxyAsync<IObjectManager>()).Returns(Task.FromResult(_objectManagerMock.Object));
			serviceFactoryMock2.Setup(x => x.CreateProxyAsync<IObjectManager>()).Returns(Task.FromResult(_objectManagerMock.Object));

			containerBuilder.RegisterInstance(serviceFactoryMock.Object).As<IDestinationServiceFactoryForUser>();
			containerBuilder.RegisterInstance(serviceFactoryMock2.Object).As<ISourceServiceFactoryForUser>();
			containerBuilder.RegisterType<DestinationWorkspaceTagsCreationExecutor>().As<IExecutor<IDestinationWorkspaceTagsCreationConfiguration>>();

			CorrelationId correlationId = new CorrelationId(Guid.NewGuid().ToString());

			containerBuilder.RegisterInstance(new EmptyLogger()).As<ISyncLog>();
			containerBuilder.RegisterInstance(correlationId).As<CorrelationId>();

			IContainer container = containerBuilder.Build();
			_executor = container.Resolve<IExecutor<IDestinationWorkspaceTagsCreationConfiguration>>();
		}

		[Test]
		public void ItShouldCreateSourceCaseTagIfItDoesNotExist()
		{
			const int sourceWorkspaceArtifactID = 1014853;
			const int destinationWorkspaceArtifactID = 1014854;
			const int jobArtifactId = 101000;
			const int sourceWorkspaceArtifactTypeId = 102001;
			const int sourceJobArtifactTypeId = 102002;
			const int newSourceWorkspaceTagArtifactId = 103000;
			const int newSourceJobTagArtifactId = 103000;
			string sourceWorkspaceName = "Cool Workspace";
			string destinationWorkspaceName = "Even Cooler Workspace";
			string jobHistoryName = "Cool Job";

			var configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactID,
				DestinationWorkspaceArtifactId = destinationWorkspaceArtifactID,
				JobArtifactId = jobArtifactId,
				SourceWorkspaceArtifactTypeId = sourceWorkspaceArtifactTypeId,
				SourceJobArtifactTypeId = sourceJobArtifactTypeId

			};

			_objectManagerMock.Setup(x => x.QueryAsync(
				-1,
				It.Is<QueryRequest>(y => y.Condition.Contains(sourceWorkspaceArtifactID.ToString(CultureInfo.InvariantCulture))),
				It.IsAny<int>(),
				It.IsAny<int>(),
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>())
			).Returns(Task.FromResult(new QueryResult
			{
				Objects = new List<RelativityObject> { new RelativityObject() { Name = sourceWorkspaceName } }
			}));

			_objectManagerMock.Setup(x => x.QueryAsync(
				-1,
				It.Is<QueryRequest>(y => y.Condition.Contains(destinationWorkspaceArtifactID.ToString(CultureInfo.InvariantCulture))),
				It.IsAny<int>(),
				It.IsAny<int>(),
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>())
			).Returns(Task.FromResult(new QueryResult { Objects = new List<RelativityObject> { new RelativityObject() { Name = destinationWorkspaceName } } }));

			_objectManagerMock.Setup(x => x.QueryAsync(
				destinationWorkspaceArtifactID,
				It.Is<QueryRequest>(y => y.ObjectType.Guid.Equals(SourceCaseTagObjectTypeGuid)),
				0,
				1,
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>())
			).Returns(Task.FromResult(new QueryResult()));

			_objectManagerMock.Setup(x => x.CreateAsync(
				destinationWorkspaceArtifactID,
				It.Is<CreateRequest>(y => y.ObjectType.Guid.Equals(SourceCaseTagObjectTypeGuid)))
			).Returns(Task.FromResult(new CreateResult { Object = new RelativityObject { ArtifactID = newSourceWorkspaceTagArtifactId } })
			).Verifiable();

			_objectManagerMock.Setup(x => x.QueryAsync(
				sourceWorkspaceArtifactID,
				It.Is<QueryRequest>(y => y.Condition.Contains(jobArtifactId.ToString(CultureInfo.InvariantCulture))),
				0,
				1,
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>()
			)).Returns(Task.FromResult(new QueryResult() { Objects = new List<RelativityObject> { new RelativityObject() { Name = jobHistoryName } } }));

			_objectManagerMock.Setup(x => x.CreateAsync(
				destinationWorkspaceArtifactID,
				It.Is<CreateRequest>(y => y.ObjectType.Guid.Equals(SourceJobTagObjectType))
			)).Returns(Task.FromResult(new CreateResult { Object = new RelativityObject { ArtifactID = newSourceJobTagArtifactId } }));

			// Act
			_executor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();

			// Assert
			_objectManagerMock.Verify();
			Assert.AreEqual(newSourceWorkspaceTagArtifactId, configuration.SourceWorkspaceTagArtifactId);
			Assert.AreEqual(newSourceJobTagArtifactId, configuration.SourceJobTagArtifactId);
		}

		[Test]
		public void ItShouldUpdateSourceCaseTagIfItDoesExist()
		{
			const int sourceWorkspaceArtifactID = 1014853;
			const int destinationWorkspaceArtifactID = 1014854;
			const int jobArtifactId = 101000;
			const int sourceWorkspaceArtifactTypeId = 102001;
			const int sourceJobArtifactTypeId = 102002;
			const int newSourceWorkspaceTagArtifactId = 103000;
			const int newSourceJobTagArtifactId = 103000;
			string sourceWorkspaceName = "Cool Workspace";
			string destinationWorkspaceName = "Even Cooler Workspace";
			string jobHistoryName = "Cool Job";

			var configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactID,
				DestinationWorkspaceArtifactId = destinationWorkspaceArtifactID,
				JobArtifactId = jobArtifactId,
				SourceWorkspaceArtifactTypeId = sourceWorkspaceArtifactTypeId,
				SourceJobArtifactTypeId = sourceJobArtifactTypeId

			};

			_objectManagerMock.Setup(x => x.QueryAsync(
				-1,
				It.Is<QueryRequest>(y => y.Condition.Contains(sourceWorkspaceArtifactID.ToString(CultureInfo.InvariantCulture))),
				It.IsAny<int>(),
				It.IsAny<int>(),
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>())
			).Returns(Task.FromResult(new QueryResult
			{
				Objects = new List<RelativityObject> { new RelativityObject() { Name = sourceWorkspaceName } }
			}));

			_objectManagerMock.Setup(x => x.QueryAsync(
				-1,
				It.Is<QueryRequest>(y => y.Condition.Contains(destinationWorkspaceArtifactID.ToString(CultureInfo.InvariantCulture))),
				It.IsAny<int>(),
				It.IsAny<int>(),
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>())
			).Returns(Task.FromResult(new QueryResult { Objects = new List<RelativityObject> { new RelativityObject() { Name = destinationWorkspaceName } } }));

			_objectManagerMock.Setup(x => x.QueryAsync(
				destinationWorkspaceArtifactID,
				It.Is<QueryRequest>(y => y.ObjectType.Guid.Equals(SourceCaseTagObjectTypeGuid)),
				0,
				1,
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>())
			).Returns(Task.FromResult(new QueryResult()));

			_objectManagerMock.Setup(x => x.CreateAsync(
				destinationWorkspaceArtifactID,
				It.Is<CreateRequest>(y => y.ObjectType.Guid.Equals(SourceCaseTagObjectTypeGuid)))
			).Returns(Task.FromResult(new CreateResult { Object = new RelativityObject { ArtifactID = newSourceWorkspaceTagArtifactId } })
			).Verifiable();

			_objectManagerMock.Setup(x => x.QueryAsync(
				sourceWorkspaceArtifactID,
				It.Is<QueryRequest>(y => y.Condition.Contains(jobArtifactId.ToString(CultureInfo.InvariantCulture))),
				0,
				1,
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>()
			)).Returns(Task.FromResult(new QueryResult() { Objects = new List<RelativityObject> { new RelativityObject() { Name = jobHistoryName } } }));

			_objectManagerMock.Setup(x => x.CreateAsync(
				destinationWorkspaceArtifactID,
				It.Is<CreateRequest>(y => y.ObjectType.Guid.Equals(SourceJobTagObjectType))
			)).Returns(Task.FromResult(new CreateResult { Object = new RelativityObject { ArtifactID = newSourceJobTagArtifactId } }));

			// Act
			_executor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();

			// Assert
			_objectManagerMock.Verify();
			Assert.AreEqual(newSourceWorkspaceTagArtifactId, configuration.SourceWorkspaceTagArtifactId);
			Assert.AreEqual(newSourceJobTagArtifactId, configuration.SourceJobTagArtifactId);
		}
	}
}