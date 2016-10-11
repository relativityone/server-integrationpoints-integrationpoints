using System;
using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;
using kCura.Injection;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Data;
using NUnit.Framework;
using OpenQA.Selenium;

namespace kCura.IntegrationPoints.Core.Tests.Integration
{
	[TestFixture]
	[Ignore("Tests don't work and need fix")]
	public class ScheduledJobStopState : RelativityProviderTemplate
	{
		private ISerializer _serializer;
		private IJobHistoryService _jobHistoryService;
		private IJobService _jobService;
		private IWebDriver _webDriver = null;
		private IObjectTypeRepository _objectTypeRepository;
		private IQueueDBContext _queueContext;
		private IJobManager _jobManager;
		private int _integrationPointArtifactTypeId;
		private long _jobId = 0;

		public ScheduledJobStopState() : base("ScheduledJobStopState Source", null)
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			_serializer = Container.Resolve<ISerializer>();
			_jobHistoryService = Container.Resolve<IJobHistoryService>();
			_jobService = Container.Resolve<IJobService>();
			_objectTypeRepository = Container.Resolve<IObjectTypeRepository>();
			_queueContext = new QueueDBContext(Helper, GlobalConst.SCHEDULE_AGENT_QUEUE_TABLE_NAME);
			_jobManager = Container.Resolve<IJobManager>();
			_integrationPointArtifactTypeId = _objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(new Guid(ObjectTypeGuids.IntegrationPoint));
		}

		public override void TestTeardown()
		{
			_webDriver.CloseSeleniumBrowser();
			_jobManager.DeleteJob(_jobId);
		}

		[TestCase(TestBrowser.Chrome)]
		public void ScheduledJobStop(TestBrowser browser)
		{
			InjectionPoint cansu1 = new InjectionPoint("E702D4CF-0468-4FEA-BzA8D-6C8C20ED91F4", "", "RIP");
			kCura.Injection.Injection cansu = new kCura.Injection.Injection(cansu1, new kCura.Injection.Behavior.InfiniteLoop(), "loop");
			try
			{
				//Arrange
				kCura.IntegrationPoints.Injection.InjectionHelper.InitializeAndEnableInjectionPoints(new List<kCura.Injection.Injection>() { cansu });

				//IntegrationModel integrationModel = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly, "testing", "Append Only");
				IntegrationModel integrationModel = CreateDefaultIntegrationPointModelScheduled(ImportOverwriteModeEnum.AppendOnly, "testing", "Append Only", "01/01/2016", "01/01/2017", kCura.ScheduleQueue.Core.ScheduleRules.ScheduleInterval.Daily);
				IntegrationModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);

				kCura.IntegrationPoints.Injection.InjectionHelper.WaitUntilInjectionPointIsReached("E702D4CF-0468-4FEA-BA8D-6C8C20ED91F4", DateTime.Now);

				Guid batchInstance = Guid.NewGuid();

				//click stop
				kCura.IntegrationPoints.Injection.InjectionHelper.RemoveInjectionFromEnvironment("E702D4CF-0468-4FEA-BA8D-6C8C20ED91F4");

				//Assert
				//Verify that after job is stopped, Job exists in the queuetable (based on end date), Job.StopState is set to 0
				//verify that next runtime is set.
			}
			finally
			{
				kCura.IntegrationPoints.Injection.InjectionHelper.CleanupInjectionPoints(new List<kCura.Injection.InjectionPoint>() { cansu1 });
			}
		}
	}
}