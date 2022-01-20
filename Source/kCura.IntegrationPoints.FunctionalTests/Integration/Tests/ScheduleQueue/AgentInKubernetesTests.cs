using kCura.IntegrationPoints.Domain.Toggles;
using NUnit.Framework;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using System;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.ScheduleQueue
{
    public class AgentInKubernetesTests : TestsBase
    {
        [Test]
        public void Agent_ShouldNotCallGetListOfResourceGroupIDs_WhenKubernetesModeIsEnabled()
        {
            // Arrange
            Context.ToggleValues.SetValue<EnableKubernetesMode>(true);

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
