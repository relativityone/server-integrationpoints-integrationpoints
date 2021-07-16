using System;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.API;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Models;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations
{
    internal class PerformanceTestsImplementation
    {
        private readonly ITestsImplementationTestFixture _testsImplementationTestFixture;
        private Workspace[] _destinationWorkspaces;
        private readonly IRipApi _ripApi = new RipApi(RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory);

        public Workspace SourceWorkspace => _testsImplementationTestFixture.Workspace; 

        public PerformanceTestsImplementation(ITestsImplementationTestFixture testsImplementationTestFixture)
        {
            _testsImplementationTestFixture = testsImplementationTestFixture;
        }

        public void OnSetUpFixture()
        {
            RelativityFacade.Instance.ImportDocumentsFromCsv(_testsImplementationTestFixture.Workspace,
                LoadFilesGenerator.GetOrCreateNativesLoadFile());

            RelativityFacade.Instance.RequireAgent(Const.INTEGRATION_POINTS_AGENT_TYPE_NAME,
                Const.INTEGRATION_POINTS_AGENT_RUN_INTERVAL);
            
            _destinationWorkspaces = Enumerable.Range(0, PerformanceTestsConstants.RUN_COUNT)
                .Select(i =>
                    RelativityFacade.Instance.CreateWorkspace(
                        string.Format(PerformanceTestsConstants.PERFORMANCE_TEST_WORKSPACE_NAME_FORMAT, i)))
                .ToArray();
        }
        
        public void OnTearDownFixture()
        {
            foreach (var destinationWorkspace in _destinationWorkspaces)
            {
                RelativityFacade.Instance.DeleteWorkspace(destinationWorkspace);
            }
            
            RelativityFacade.Instance.DeleteWorkspace(SourceWorkspace);
        }

        /// <summary>
        /// Returns measured time in seconds. Measures only time between job start and finish
        ///
        /// Job is started when status changes from Pending to anything else
        /// Job is ended when status is not Processing or Validating
        /// </summary>
        /// <returns>job time in seconds</returns>
        public async Task<double> RunSyncAndMeasureTime(int runIndex)
        {
            Workspace destinationWorkspace = _destinationWorkspaces[runIndex];

            IntegrationPointModel integrationPoint = GetIntegrationPoint(destinationWorkspace);
            await _ripApi.CreateIntegrationPoint(integrationPoint, SourceWorkspace.ArtifactID).ConfigureAwait(false);

            int jobId = await _ripApi.RunIntegrationPoint(integrationPoint, SourceWorkspace.ArtifactID).ConfigureAwait(false);

            await WaitForJobToStart(jobId).ConfigureAwait(false);
            DateTime startTime = DateTime.Now;

            await WaitForJobToFinish(jobId);
            DateTime endTime = DateTime.Now;

            return (endTime - startTime).TotalSeconds;
        }

        /// <summary>
        /// Runs <see cref="RunSyncAndMeasureTime"/> in a loop and calculates average time
        /// </summary>
        /// <returns>Average execution time</returns>
        public async Task<double> RunPerformanceBenchmark()
        {
            double runTimeSum = 0;
            for (int i = 0; i < PerformanceTestsConstants.RUN_COUNT; i++)
            {
                runTimeSum += await RunSyncAndMeasureTime(i).ConfigureAwait(false);
            }

            return runTimeSum / PerformanceTestsConstants.RUN_COUNT;
        }

        private Task WaitForJobToFinish(int jobId)
        {
            return WaitForJobStatus(jobId,
                status => 
                    status != PerformanceTestsConstants.JOB_STATUS_PROCESSING &&
                    status != PerformanceTestsConstants.JOB_STATUS_VALIDATING);
        }

        private Task WaitForJobToStart(int jobId)
        {
            return WaitForJobStatus(jobId, status => status != PerformanceTestsConstants.JOB_STATUS_PENDING);
        }

        private async Task WaitForJobStatus(int jobId, Func<string, bool> waitUntil)
        {
            string status = await _ripApi.CheckJobStatus(jobId);
            while (!waitUntil(status))
            {
                await Task.Delay(500);
                status = await _ripApi.CheckJobStatus(jobId).ConfigureAwait(false);
            }
        }

        private IntegrationPointModel GetIntegrationPoint(Workspace destinationWorkspace)
        {
            return new IntegrationPointModel();
        }
    }
}