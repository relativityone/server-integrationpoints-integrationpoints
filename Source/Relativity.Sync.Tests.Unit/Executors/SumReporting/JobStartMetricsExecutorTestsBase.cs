using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Executors.SumReporting
{
    internal abstract class JobStartMetricsExecutorTestsBase
    {
        protected Mock<ISyncLog> SyncLogMock;
        protected Mock<ISyncMetrics> SyncMetricsMock;

        protected Mock<IFieldManager> FieldManagerFake;
        protected Mock<IObjectManager> ObjectManagerFake;

        protected Mock<ISourceServiceFactoryForUser> ServiceFactory;

        protected const int SOURCE_WORKSPACE_ARTIFACT_ID = 1;
        protected const int DESTINATION_WORKSPACE_ARTIFACT_ID = 2;

        [SetUp]
        public virtual void SetUp()
        {
            SyncLogMock = new Mock<ISyncLog>();

            SyncMetricsMock = new Mock<ISyncMetrics>();

            FieldManagerFake = new Mock<IFieldManager>();

            ObjectManagerFake = new Mock<IObjectManager>(MockBehavior.Strict);
            ObjectManagerFake.Setup(x => x.Dispose());

            ServiceFactory = new Mock<ISourceServiceFactoryForUser>();
            ServiceFactory.Setup(x => x.CreateProxyAsync<IObjectManager>())
                .ReturnsAsync(ObjectManagerFake.Object);
        }

        protected void SetupFieldMapping(IEnumerable<FieldMapDefinitionCase> mapping)
		{
			int artifactIdCounter = 1;
			ObjectManagerFake
				.Setup(x => x.QuerySlimAsync(SOURCE_WORKSPACE_ARTIFACT_ID, It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new QueryResultSlim
				{
					Objects = mapping.Select(x =>
						new RelativityObjectSlim
						{
							ArtifactID = artifactIdCounter++,
							Values = new List<object> { x.SourceFieldName, x.SourceFieldDataGridEnabled }
						}
					).ToList()
				});

			ObjectManagerFake
				.Setup(x => x.QuerySlimAsync(DESTINATION_WORKSPACE_ARTIFACT_ID, It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new QueryResultSlim
				{
					Objects = mapping.Select(x =>
						new RelativityObjectSlim
						{
							ArtifactID = artifactIdCounter++,
							Values = new List<object> { x.DestinationFieldName, x.DestinationFieldDataGridEnabled }
						}
					).ToList()
				});


			FieldManagerFake.Setup(x => x.GetMappedFieldsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mapping.Select(x =>
					new FieldInfoDto(x.SpecialFieldType, x.SourceFieldName, x.DestinationFieldName, true, true) { RelativityDataType = x.DataType }
				).ToList);
		}

		internal class FieldMapDefinitionCase
		{
			public string SourceFieldName { get; set; }
			public string DestinationFieldName { get; set; }
			public RelativityDataType DataType { get; set; }
			public bool SourceFieldDataGridEnabled { get; set; }
			public bool DestinationFieldDataGridEnabled { get; set; }
			public SpecialFieldType SpecialFieldType { get; set; } = SpecialFieldType.None;
		}
    }
}
