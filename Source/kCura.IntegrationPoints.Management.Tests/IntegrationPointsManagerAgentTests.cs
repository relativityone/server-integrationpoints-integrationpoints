using System;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Management.Installers;
using kCura.IntegrationPoints.Manager;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Management.Tests
{
	[TestFixture]
	public class IntegrationPointsManagerAgentTests : TestBase
	{
		private IntegrationPointsManagerAgent _integrationPointsManagerAgent;
		private IWindsorContainer _container;
		private IAPILog _logger;

		public override void SetUp()
		{
			_logger = Substitute.For<IAPILog>();
			var containerFactory = Substitute.For<IContainerFactory>();

			_container = Substitute.For<IWindsorContainer>();
			containerFactory.Create(Arg.Any<IAgentHelper>()).Returns(_container);

			_integrationPointsManagerAgent = new IntegrationPointsManagerAgentMock(_logger, containerFactory);
		}

		[Test]
		public void ItShouldStartManager()
		{
			var manager = Substitute.For<IIntegrationPointsManager>();

			_container.Resolve<IIntegrationPointsManager>().Returns(manager);

			// ACT
			_integrationPointsManagerAgent.Execute();

			// ASSERT
			manager.Received(1).Start();
		}

		[Test]
		public void ItShouldLogError()
		{
			_container.When(x => x.Resolve<IIntegrationPointsManager>()).Throw<Exception>();

			// ACT
			_integrationPointsManagerAgent.Execute();

			// ASSERT
			_logger.Received(1).LogError(Arg.Any<Exception>(), Arg.Any<string>());
		}

		internal class IntegrationPointsManagerAgentMock : IntegrationPointsManagerAgent
		{
			internal override IAPILog Logger { get; }

			public IntegrationPointsManagerAgentMock(IAPILog logger, IContainerFactory containerFactory) : base(containerFactory)
			{
				Logger = logger;
			}
		}
	}
}