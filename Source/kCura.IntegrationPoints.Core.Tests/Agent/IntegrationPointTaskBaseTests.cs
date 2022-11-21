using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Agent;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Logging;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Core.Tests.Agent
{
    [TestFixture, Category("Unit")]
    public class IntegrationPointTaskBaseTests : TestBase
    {
        private TestClass _testInstance;

        protected ICaseServiceContext _caseServiceContext;
        protected IHelper _helper;
        protected IDataProviderFactory _dataProviderFactory;
        protected Apps.Common.Utils.Serializers.ISerializer _serializer;
        protected IJobHistoryService _jobHistoryService;
        protected JobHistoryErrorService _jobHistoryErrorService;
        protected ISynchronizerFactory _appDomainRdoSynchronizerFactoryFactory;
        protected IJobManager _jobManager;
        protected IManagerFactory _managerFactory;
        protected IJobService _jobService;
        protected IIntegrationPointService _integrationPointService;


        [SetUp]
        public override void SetUp()
        {
            _caseServiceContext = Substitute.For<ICaseServiceContext>();
            _helper = Substitute.For<IHelper>();
            _jobService = Substitute.For<IJobService>();
            _managerFactory = Substitute.For<IManagerFactory>();
            _serializer = Substitute.For<kCura.Apps.Common.Utils.Serializers.ISerializer>();
            _dataProviderFactory = Substitute.For<IDataProviderFactory>();
            _appDomainRdoSynchronizerFactoryFactory = Substitute.For<ISynchronizerFactory>();
            _integrationPointService = Substitute.For<IIntegrationPointService>();

            // Stubs
            _testInstance = new TestClass(_caseServiceContext,
                _helper,
                _dataProviderFactory,
                _serializer,
                _appDomainRdoSynchronizerFactoryFactory,
                _jobHistoryService,
                _jobHistoryErrorService,
                _jobManager,
                _managerFactory,
                _jobService,
                _integrationPointService);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(";")]
        [TestCase(" ;")]
        [TestCase("; ")]
        [TestCase(" ; ")]
        [TestCase(";;")]
        [TestCase(" ; ; ")]
        public void GetRecipientEmails_NoEmailTests(string testEmail)
        {
            // ARRANGE
            var integrationPoint = new IntegrationPointDto
            {
                EmailNotificationRecipients = testEmail,
            };

            _testInstance.SetIntegrationPoint(integrationPoint);

            // ACT
            List<string> resultEmails = _testInstance.GetRecipientEmails();

            // ASSERT
            Assert.IsNotNull(resultEmails);
            Assert.AreEqual(0, resultEmails.Count);

        }

        [Test]
        public void GetRecipientEmails_OneEmailTest()
        {
            // ARRANGE
            string email1 = "email1@relativity.com";

            var integrationPoint = new IntegrationPointDto
            {
                EmailNotificationRecipients = email1,
            };

            _testInstance.SetIntegrationPoint(integrationPoint);

            // ACT
            List<string> resultEmails = _testInstance.GetRecipientEmails();

            // ASSERT
            Assert.IsNotNull(resultEmails);
            Assert.AreEqual(1, resultEmails.Count);
            Assert.AreEqual(email1, resultEmails[0]);
        }

        [Test]
        public void GetRecipientEmails_TwoEmailTest()
        {
            // ARRANGE
            string email1 = "email1@relativity.com";
            string email2 = "email2@relativity.com";

            var integrationPoint = new IntegrationPointDto
            {
                EmailNotificationRecipients = $"  {email1} ;   {email2}   ",
            };

            _testInstance.SetIntegrationPoint(integrationPoint);

            // ACT
            List<string> resultEmails = _testInstance.GetRecipientEmails();

            // ASSERT
            Assert.IsNotNull(resultEmails);
            Assert.AreEqual(2, resultEmails.Count);
            Assert.AreEqual(email1, resultEmails[0]);
            Assert.AreEqual(email2, resultEmails[1]);
        }

        [Test]
        public void GetRecipientEmails_MultipleEmailTest()
        {
            // ARRANGE
            string email1 = "email1@relativity.com";
            string email2 = "email2@relativity.com";
            string email3 = "email3@relativity.com";

            var integrationPoint = new IntegrationPointDto
            {
                EmailNotificationRecipients = $"  {email1} ;  ;;;  ;;;;  {email2}   ;  {email3} ;;;",
            };

            _testInstance.SetIntegrationPoint(integrationPoint);

            // ACT
            List<string> resultEmails = _testInstance.GetRecipientEmails();

            // ASSERT
            Assert.IsNotNull(resultEmails);
            Assert.AreEqual(3, resultEmails.Count);
            Assert.AreEqual(email1, resultEmails[0]);
            Assert.AreEqual(email2, resultEmails[1]);
            Assert.AreEqual(email3, resultEmails[2]);
        }

        [Test]
        public void GetSourceProvider_GoldFlow()
        {
            // ARRANGE
            const string jobDetailsText = "SERIALIZED";
            const long jobIdValue = 12321;
            var sourceProvider = new SourceProvider()
            {
                ApplicationIdentifier = Guid.NewGuid().ToString(),
                Identifier = Guid.NewGuid().ToString()
            };

            IDataSourceProvider expectedDataSourceProvider = Substitute.For<IDataSourceProvider>();
            IJobStopManager jobStopManager = Substitute.For<IJobStopManager>();

            var taskParameters = new TaskParameters()
            {
                BatchInstance = Guid.NewGuid(),
            };

            Job job = JobHelper.GetJob(jobIdValue, null, null, 0, 0, 0, 0, TaskType.SyncWorker, DateTime.Now, null,
                jobDetailsText, 0, DateTime.Now, 0, String.Empty, String.Empty);

            _serializer.Deserialize<TaskParameters>(Arg.Is<string>(x => x.Equals(jobDetailsText))).Returns(taskParameters);

            _managerFactory.CreateJobStopManager(
                    Arg.Is(_jobService),
                    Arg.Is(_jobHistoryService),
                    Arg.Is(taskParameters.BatchInstance),
                    jobIdValue,
                    true,
                    Arg.Any<IDiagnosticLog>())
                .Returns(jobStopManager);

            _dataProviderFactory.GetDataProvider(Arg.Is(new Guid(sourceProvider.ApplicationIdentifier)),
                Arg.Is(new Guid(sourceProvider.Identifier))).Returns(expectedDataSourceProvider);

            // ACT
            IDataSourceProvider result = _testInstance.GetSourceProvider(sourceProvider, job);

            // ASSERT
            Assert.AreEqual(expectedDataSourceProvider, result);
            _dataProviderFactory.Received(1).GetDataProvider(Arg.Is(new Guid(sourceProvider.ApplicationIdentifier)),
                Arg.Is(new Guid(sourceProvider.Identifier)));
        }

        [Test]
        public void GetDestinationProvider_GoldFlow()
        {
            // ARRANGE
            const string jobDetailsText = "SERIALIZED";
            const long jobIdValue = 12321;
            const string configuration = "config";
            IJobStopManager jobStopManager = Substitute.For<IJobStopManager>();
            var destinationProvider = new DestinationProvider()
            {
                Identifier = Guid.NewGuid().ToString()
            };

            IDataSynchronizer expectedDataSynchronizer = Substitute.For<IDataSynchronizer>();

            var taskParameters = new TaskParameters()
            {
                BatchInstance = Guid.NewGuid(),
            };

            Job job = JobHelper.GetJob(jobIdValue, null, null, 0, 0, 0, 0, TaskType.SyncWorker, DateTime.Now, null,
                jobDetailsText, 0, DateTime.Now, 0, String.Empty, String.Empty);

            _serializer.Deserialize<TaskParameters>(Arg.Is<string>(x => x.Equals(jobDetailsText))).Returns(taskParameters);

            _managerFactory.CreateJobStopManager(Arg.Is(_jobService), Arg.Is(_jobHistoryService),
                Arg.Is(taskParameters.BatchInstance), jobIdValue, true, Arg.Any<IDiagnosticLog>())
                .Returns(jobStopManager);

            _appDomainRdoSynchronizerFactoryFactory.CreateSynchronizer(Arg.Is(new Guid(destinationProvider.Identifier)), Arg.Is(configuration))
                .Returns(expectedDataSynchronizer);

            // ACT
            IDataSynchronizer result = _testInstance.GetDestinationProvider(destinationProvider, configuration, job);

            // ASSERT
            Assert.AreEqual(expectedDataSynchronizer, result);
            _appDomainRdoSynchronizerFactoryFactory.Received(1).CreateSynchronizer(Arg.Is(new Guid(destinationProvider.Identifier)), Arg.Is(configuration));
        }
    }

    public class TestClass : IntegrationPointTaskBase
    {
        public TestClass(
            ICaseServiceContext caseServiceContext,
            IHelper helper,
            IDataProviderFactory dataProviderFactory,
            Apps.Common.Utils.Serializers.ISerializer serializer,
            ISynchronizerFactory appDomainRdoSynchronizerFactoryFactory,
            IJobHistoryService jobHistoryService,
            JobHistoryErrorService jobHistoryErrorService,
            IJobManager jobManager,
            IManagerFactory managerFactory,
            IJobService jobService,
            IIntegrationPointService integrationPointService)
            : base(
                caseServiceContext,
                helper,
                dataProviderFactory,
                serializer,
                appDomainRdoSynchronizerFactoryFactory,
                jobHistoryService,
                jobHistoryErrorService,
                jobManager,
                managerFactory,
                jobService,
                integrationPointService,
                new EmptyDiagnosticLog())
        {
        }

        public void SetIntegrationPoint(IntegrationPointDto dto)
        {
            IntegrationPointDto = dto;
        }

        public new List<string> GetRecipientEmails()
        {
            return base.GetRecipientEmails();
        }

        public new IDataSourceProvider GetSourceProvider(SourceProvider sourceProviderRdo, Job job)
        {
            return base.GetSourceProvider(sourceProviderRdo, job);
        }

        public new IDataSynchronizer GetDestinationProvider(DestinationProvider destinationProviderRdo, string configuration,
            Job job)
        {
            return base.GetDestinationProvider(destinationProviderRdo, configuration, job);
        }
    }
}
