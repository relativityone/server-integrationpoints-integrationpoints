using FluentAssertions;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services;
using Relativity.Testing.Identification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Keplers
{
    public class JobHistoryManagerTests : TestsBase
    {
        private IJobHistoryManager _manager;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _manager = Container.Resolve<IJobHistoryManager>();
        }

        [IdentifiedTest("21FECB87-6307-43E9-900A-9119C94380DC")]
        public async Task ItShouldReturnJobHistory()
        {
            //Arrange
            JobHistoryRequest request = new JobHistoryRequest { WorkspaceArtifactId = SourceWorkspace.ArtifactId };
            //SourceWorkspace.Helpers.IntegrationPointHelper.CreateEmptyIntegrationPoint();

            //Act
            JobHistorySummaryModel jobHistoryModel = await _manager.GetJobHistoryAsync(request).ConfigureAwait(false);

            //Assert
            jobHistoryModel.Should().NotBeNull();
        }



    }
}
