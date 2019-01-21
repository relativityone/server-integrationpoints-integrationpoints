using FluentAssertions;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Data.Facades;
using kCura.IntegrationPoints.Data.Facades.Implementations;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Interfaces;

namespace kCura.IntegrationPoints.Data.Tests.Facades.Implementations
{
	[TestFixture]
	public class ObjectManagerFacadeFactoryTests
	{
		private ObjectManagerFacadeFactory _sut;
		private Mock<IAPILog> _logger;
		private Mock<IServicesMgr> _servicesManager;
		private Mock<IExternalServiceInstrumentationProvider> _instrumentationProvider;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_logger = new Mock<IAPILog>();

			var instrumentationStarted = new Mock<IExternalServiceInstrumentationStarted>();
			var instrumentation = new Mock<IExternalServiceInstrumentation>();
			instrumentation.Setup(x => x.Started()).Returns(instrumentationStarted.Object);
			_instrumentationProvider = new Mock<IExternalServiceInstrumentationProvider>();
			_instrumentationProvider
				.Setup(x => x.Create(
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<string>()))
				.Returns(instrumentation.Object);
		}

		[SetUp]
		public void SetUp()
		{
			var objectManager = new Mock<IObjectManager>();
			_servicesManager = new Mock<IServicesMgr>();
			_servicesManager.Setup(x => x.CreateProxy<IObjectManager>(It.IsAny<ExecutionIdentity>())).Returns(objectManager.Object);

			var retryHandler = new RetryHandler(null, 0, 0);
			var retryHandlerFactory = new Mock<IRetryHandlerFactory>();
			retryHandlerFactory
				.Setup(x => x.Create(It.IsAny<ushort>(), It.IsAny<ushort>()))
				.Returns(retryHandler);

			_sut = new ObjectManagerFacadeFactory(_servicesManager.Object, _logger.Object, _instrumentationProvider.Object, retryHandlerFactory.Object);
		}

		[Test]
		public void ShouldCreateFacadeWithRetries()
		{
			// act
			IObjectManagerFacade createdInstance = _sut.Create(ExecutionIdentity.Manual);

			// assert
			createdInstance.Should().BeOfType<ObjectManagerFacadeRetryDecorator>();
		}

		[TestCase(ExecutionIdentity.Manual)]
		[TestCase(ExecutionIdentity.CurrentUser)]
		[TestCase(ExecutionIdentity.System)]
		public async Task CreatedInstanceShouldRequestForServiceWithProperExecutionIdentity(ExecutionIdentity executionIdentity)
		{
			// arrange
			IObjectManagerFacade createdInstance = _sut.Create(executionIdentity);

			// act
			await createdInstance.DeleteAsync(0, null);

			// assert
			_servicesManager.Verify(x => x.CreateProxy<IObjectManager>(executionIdentity));
		}
	}
}
