using NUnit.Framework;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using System;
using kCura.IntegrationPoints.Domain.EnvironmentalVariables;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.ScheduleQueue
{
    public class AgentInKubernetesTests : TestsBase
    {
        [Test]
        public void Agent_ShouldNotCallGetListOfResourceGroupIDs_WhenKubernetesModeIsEnabled()
        {
            // Arrange
            IKubernetesMode kubernetesMode = Container.Resolve<IKubernetesMode>();
            kubernetesMode.IsEnabled = true;

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
