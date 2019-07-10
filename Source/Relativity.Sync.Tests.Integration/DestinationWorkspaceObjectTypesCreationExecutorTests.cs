using Autofac;
using NUnit.Framework;
using Relativity.Sync.Executors;
using Relativity.Sync.Logging;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	public class DestinationWorkspaceObjectTypesCreationExecutorTests
	{
		private ContainerBuilder _containerBuilder;
		private IContainer _container;
		private DestinationWorkspaceObjectTypesCreationExecutor _instance;
		private DocumentTransferServicesMocker _documentTransferServicesMocker;
		private ConfigurationStub _configuration;

		private const int _WORKSPACE_ID = 1;

		[SetUp]
		public void SetUp()
		{
			_configuration = new ConfigurationStub
			{
				DestinationWorkspaceArtifactId = _WORKSPACE_ID
			};

			_documentTransferServicesMocker = new DocumentTransferServicesMocker();
			_containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
			IntegrationTestsContainerBuilder.MockReporting(_containerBuilder);
			_documentTransferServicesMocker.RegisterServiceMocks(_containerBuilder);
			_containerBuilder.RegisterInstance(_configuration).AsImplementedInterfaces();
			_container = _containerBuilder.Build();

			IFieldManager fieldManager = _container.Resolve<IFieldManager>();
			_documentTransferServicesMocker.SetFieldManager(fieldManager);

			_instance = new DestinationWorkspaceObjectTypesCreationExecutor(_container.Resolve<ISyncObjectTypeManager>(),
				_container.Resolve<ISyncFieldManager>(), new EmptyLogger());
		}
	}
}