using NUnit.Framework;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using System;
using kCura.IntegrationPoints.Domain.EnvironmentalVariables;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.ScheduleQueue
{
    public class AgentInKubernetesTests : TestsBase
    {
        [SetUp]
        public void Setup()
        {
            FakeKubernetesMode kubernetesMode = (FakeKubernetesMode)Container.Resolve<IKubernetesMode>();
            kubernetesMode.SetIsEnabled(true);
        }

        [Test]
        public void Agent_ShouldNotCallGetListOfResourceGroupIDs_WhenKubernetesModeIsEnabled()
        {
            // Arrange
            JobTest job = FakeRelativityInstance.Helpers.JobHelper.ScheduleBasicJob(SourceWorkspace);

            FakeAgent sut = FakeAgent.Create(FakeRelativityInstance, Container, shouldRunOnce: true);

            sut.GetResourceGroupIDsMockFunc = () => throw new NotSupportedException();

            // Act
            sut.Execute();

            // Assert
            sut.VerifyJobsWereProcessed(new long[] { job.JobId });
        }
    }
}
