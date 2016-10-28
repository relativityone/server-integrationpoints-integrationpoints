using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Agent.Installer;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tests
{
	[TestFixture]
	public class CastleWindsorValidatorTests : CastleWindsorValidatorBase
	{
		private IWindsorContainer _container;
		private IAgentHelper _agentHelper;
		private IDBContext _dbContext;
		private IScheduleRuleFactory _scheduleRuleFactory;

		private const int _WORKSPACE_ID = 84092823;
		private const int _INTEGRATION_POINT_ID = 384093;
		private const int _SUBMITTED_BY_ID = 489324;

		[SetUp]
		public void SetUp()
		{
			// Set up mocks
			_agentHelper = NSubstitute.Substitute.For<IAgentHelper>();
			_scheduleRuleFactory = NSubstitute.Substitute.For<IScheduleRuleFactory>();
			_dbContext = NSubstitute.Substitute.For<IDBContext>();

			// Set up container
			_container = new WindsorContainer();

			// Register mocks
			_container.Register(Component.For<IHelper>().Instance(_agentHelper).LifestyleTransient());

			// Set up stubs
			_agentHelper.GetDBContext(-1).ReturnsForAnyArgs(_dbContext);
		}

		[Ignore("")]
		[Test]
		public void AgentInstallerInstallsSuccessfully()
		{
			// Arrange
			Job job = JobExtensions.CreateJob(_WORKSPACE_ID, _INTEGRATION_POINT_ID, _SUBMITTED_BY_ID);

			IWindsorInstaller agentInstaller = new AgentInstaller(_agentHelper, job, _scheduleRuleFactory);

			_container.Install(agentInstaller);

			// Act / Assert	
			CheckForPotentiallyMisconfiguredComponents(_container);
		}
	}
}