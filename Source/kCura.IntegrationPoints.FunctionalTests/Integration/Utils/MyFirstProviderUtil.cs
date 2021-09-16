
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using kCura.ScheduleQueue.Core.Core;
using Moq;
using Newtonsoft.Json.Linq;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.Tests.Integration.Helpers;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Services.ImportApi;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Utils
{
    public class MyFirstProviderUtil
    {
        private readonly IWindsorContainer _container;
        private readonly RelativityInstanceTest _fakeRelativityInstance;
        private readonly WorkspaceTest _sourceWorkspace;
        private readonly ISerializer _serializer;

        public MyFirstProviderUtil(IWindsorContainer container, RelativityInstanceTest fakeRelativityInstance,
            WorkspaceTest sourceWorkspace, ISerializer serializer)
        {
            _container = container;
            _fakeRelativityInstance = fakeRelativityInstance;
            _sourceWorkspace = sourceWorkspace;
            _serializer = serializer;
        }

        public string PrepareRecords(int numberOfRecords)
        {
            string xml = new MyFirstProviderXmlGenerator().GenerateRecords(numberOfRecords);
            string tmpPath = Path.GetTempFileName();
            File.WriteAllText(tmpPath, xml);
            return tmpPath;
        }

        public JobTest PrepareJobs(string xmlPath, int numberOfBatches, Action<JobTest> registerJobContext)
        {
            _fakeRelativityInstance.Helpers.AgentHelper.CreateIntegrationPointAgent();

            SourceProviderTest provider =
                _sourceWorkspace.Helpers.SourceProviderHelper.CreateMyFirstProvider();

            IntegrationPointTest integrationPoint =
                _sourceWorkspace.Helpers.IntegrationPointHelper.CreateImportIntegrationPoint(provider,
                    identifierFieldName: "Name", sourceProviderConfiguration: xmlPath);

            integrationPoint.SourceProvider = provider.ArtifactId;
            integrationPoint.SourceConfiguration = xmlPath;
            JobTest job =
                _fakeRelativityInstance.Helpers.JobHelper.ScheduleIntegrationPointRun(_sourceWorkspace,
                    integrationPoint);

            _sourceWorkspace.Helpers.JobHistoryHelper.CreateJobHistory(job, integrationPoint);

            TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(job.JobDetails);
            string[] recordsIds = XDocument.Load(xmlPath).XPathSelectElements("//Name").Select(x => x.Value)
                .ToArray();

            taskParameters.BatchParameters = recordsIds;

            job.JobDetails = _serializer.Serialize(taskParameters);

            for (int i = 1; i < numberOfBatches; i++)
            {
                JobTest newJob =
                    _fakeRelativityInstance.Helpers.JobHelper.ScheduleIntegrationPointRun(_sourceWorkspace,
                        integrationPoint);

                newJob.JobDetails = job.JobDetails; // link all jobs together with BatchInstance
            }

            registerJobContext(job);

            return job;
        }

        public JobTest PrepareJob(string xmlPath, out JobHistoryTest jobHistory
            , Action<JobTest> registerJobContext, string emailToAddress = null)
        {
            AgentTest agent = _fakeRelativityInstance.Helpers.AgentHelper.CreateIntegrationPointAgent();

            SourceProviderTest provider =
                _sourceWorkspace.Helpers.SourceProviderHelper.CreateMyFirstProvider();

            IntegrationPointTest integrationPoint =
                _sourceWorkspace.Helpers.IntegrationPointHelper.CreateImportIntegrationPoint(provider,
                    identifierFieldName: "Name", sourceProviderConfiguration: xmlPath);

            integrationPoint.SourceProvider = provider.ArtifactId;
            integrationPoint.SourceConfiguration = xmlPath;
            integrationPoint.EmailNotificationRecipients = emailToAddress;

            JobTest job =
                _fakeRelativityInstance.Helpers.JobHelper.ScheduleIntegrationPointRun(_sourceWorkspace, integrationPoint);
            jobHistory = _sourceWorkspace.Helpers.JobHistoryHelper.CreateJobHistory(job, integrationPoint);

            TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(job.JobDetails);
            string[] recordsIds = XDocument.Load(xmlPath).XPathSelectElements("//Name").Select(x => x.Value).ToArray();

            taskParameters.BatchParameters = recordsIds;

            job.JobDetails = _serializer.Serialize(taskParameters);
            job.LockedByAgentID = agent.ArtifactId;
            job.RootJobId = JobId.Next;

            InsertBatchToJobTrackerTable(job, jobHistory);

            registerJobContext(job);

            return job;
        }

        public void PrepareOtherJobs(JobTest job, JobHistoryTest jobHistory, JobTest[] otherJobs)
        {
            foreach (var otherJob in otherJobs)
            {
                otherJob.RootJobId = job.RootJobId;
                otherJob.WorkspaceID = job.WorkspaceID;

                _fakeRelativityInstance.JobsInQueue.Add(otherJob);

                InsertBatchToJobTrackerTable(otherJob, jobHistory);
            }
        }
        public SyncWorker PrepareSut(Action<FakeJobImport> importAction)
        {
            _container.Register(Component.For<IDataSourceProvider>()
                .ImplementedBy<MyFirstProvider.Provider.MyFirstProvider>()
                .Named(MyFirstProvider.Provider.GlobalConstants.FIRST_PROVIDER_GUID));

            _container.Register(Component.For<IJobImport>().Instance(new FakeJobImport(importAction))
                .LifestyleSingleton());

            SyncWorker sut = _container.Resolve<SyncWorker>();
            return sut;
        }

        public List<string> GetRemainingItems(JobTest job)
        {
            TaskParameters paramaters = _serializer.Deserialize<TaskParameters>(job.JobDetails);
            List<string> remainingItems = (paramaters.BatchParameters as JArray).ToObject<List<string>>();
            return remainingItems;
        }

        public void SetupWorkspaceDbContextMock_AsNotLastBatch()
        {
            Mock<IWorkspaceDBContext> dbContextMock = new Mock<IWorkspaceDBContext>();
            dbContextMock.Setup(x => x.ExecuteNonQuerySQLStatement(It.IsAny<string>())).Returns(1);
            dbContextMock.Setup(_ =>
                    _.ExecuteNonQuerySQLStatement(It.IsAny<string>(), It.IsAny<IEnumerable<SqlParameter>>()))
                .Returns(0);


            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("");
            dataTable.Rows.Add(new object());
            dbContextMock.Setup(x =>
                    x.ExecuteSqlStatementAsDataTable(It.IsAny<string>(), It.IsAny<IEnumerable<SqlParameter>>()))
                .Returns(dataTable);

            dbContextMock.Setup(_ => _.ExecuteSqlStatementAsScalar<int>(It.IsAny<string>(),
                    It.IsAny<IEnumerable<SqlParameter>>()))
                .Returns((string sql, IEnumerable<SqlParameter> sqlParams) =>
                {
                    return sqlParams.Any(p => p.ParameterName.Contains("batchIsFinished")) ? 1 : 0;
                });

            _container.Register(Component.For<IWorkspaceDBContext>().Instance(dbContextMock.Object).LifestyleSingleton()
                .IsDefault());
        }

        private void InsertBatchToJobTrackerTable(JobTest job, JobHistoryTest jobHistory)
        {
            string tableName = string.Format("RIP_JobTracker_{0}_{1}_{2}", job.WorkspaceID, job.RootJobId, jobHistory.BatchInstance);


            if (!_fakeRelativityInstance.JobTrackerResourceTables.ContainsKey(tableName))
            {
                _fakeRelativityInstance.JobTrackerResourceTables[tableName] = new List<JobTrackerTest>();
            }

            _fakeRelativityInstance.JobTrackerResourceTables[tableName].Add(new JobTrackerTest { JobId = job.JobId });
        }
    }
}
