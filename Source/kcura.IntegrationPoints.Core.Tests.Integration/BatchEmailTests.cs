using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Keywords;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.Core.Tests.Integration
{
	[TestFixture]
	public class BatchEmailTests
	{
		private BatchEmail _sut;
		private Mock<IJobManager> _jobManagerMock;
		private Mock<ISerializer> _serializer;
		private Mock<IJobStatusUpdater> _jobStatusUpdater;
		private Mock<IIntegrationPointRepository> _integrationPointRepository;

		[SetUp]
		public void SetUp()
		{
			Mock<ICaseServiceContext> caseServiceContext = new Mock<ICaseServiceContext>();
			Mock<IHelper> helper = new Mock<IHelper>{DefaultValue = DefaultValue.Mock};
			Mock<IDataProviderFactory> dataProviderFactory = new Mock<IDataProviderFactory>();
			Mock<ISynchronizerFactory> appDomainRdoSynchronizerFactoryFactory = new Mock<ISynchronizerFactory>();
			Mock<IJobHistoryService> jobHistoryService = new Mock<IJobHistoryService>();
			Mock<IJobHistoryErrorService> jobHistoryErrorService = new Mock<IJobHistoryErrorService>();
			Mock<IKeywordConverter> converter = new Mock<IKeywordConverter>();
			Mock<IManagerFactory> managerFactory = new Mock<IManagerFactory>();
			Mock<IJobService> jobService = new Mock<IJobService>();
			_integrationPointRepository = new Mock<IIntegrationPointRepository>();
			_serializer = new Mock<ISerializer>();
			_jobStatusUpdater = new Mock<IJobStatusUpdater>();
			_jobManagerMock = new Mock<IJobManager>();

			_sut = new BatchEmail(
				caseServiceContext.Object,
				helper.Object,
				dataProviderFactory.Object,
				_serializer.Object,
				appDomainRdoSynchronizerFactoryFactory.Object,
				jobHistoryService.Object,
				jobHistoryErrorService.Object,
				_jobManagerMock.Object,
				_jobStatusUpdater.Object,
				converter.Object,
				managerFactory.Object,
				jobService.Object,
				_integrationPointRepository.Object
				);
		}
		
		[IdentifiedTest("59CDF9B0-4C7D-447B-A7F0-C9685F48E739")]
		public void EmailJobParametersShouldHaveTheSameBatchInstanceAsParentJob()
		{
			//ARRANGE
			Data.IntegrationPoint integrationPoint = new Data.IntegrationPoint();
			integrationPoint.EmailNotificationRecipients = "email@email.com";
			_integrationPointRepository
				.Setup(x => x.ReadWithFieldMappingAsync(It.IsAny<int>()))
				.ReturnsAsync(integrationPoint);

			Guid batchInstanceGuid = Guid.NewGuid();
			string jobDetails = $"{{\"BatchInstance\":\"{batchInstanceGuid.ToString()}\",\"BatchParameters\":\"{{\"}}}}";
			TaskParameters taskParameters = new TaskParameters(){BatchInstance = batchInstanceGuid};
			_serializer.Setup(x => x.Deserialize<TaskParameters>(jobDetails)).Returns(taskParameters);
			_jobStatusUpdater.Setup(x => x.GenerateStatus(It.IsAny<Guid>()))
				.Returns(Data.JobStatusChoices.JobHistoryCompletedWithErrors);
			Job parentJob = JobHelper.GetJob(
				1, 
				null, 
				null, 
				1, 
				1, 
				111,
				222,
				TaskType.ExportManager,
				new DateTime(),
				null, 
				jobDetails,
				0, 
				new DateTime(),
				1, 
				null, 
				null
				);

			//ACT
			_sut.OnJobComplete(parentJob);

			//ASSERT
			_jobManagerMock.Verify(x => 
				x.CreateJob(
					parentJob, 
					It.Is<TaskParameters>(y =>
						y.BatchInstance == batchInstanceGuid
					), 
					TaskType.SendEmailWorker));
		}
	}
}
