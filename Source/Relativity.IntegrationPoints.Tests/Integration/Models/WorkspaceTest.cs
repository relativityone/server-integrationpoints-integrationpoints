using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Data;
using Relativity.IntegrationPoints.Tests.Integration.Helpers.WorkspaceHelpers;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.ResourceServer;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
	public class WorkspaceTest : RdoTestBase
    {
        public string Name { get; set; }

        public FileShareResourceServer FileShareServer { get; set; }

        public IList<IntegrationPointTest> IntegrationPoints { get; } = new List<IntegrationPointTest>();

        public IList<IntegrationPointTypeTest> IntegrationPointTypes { get; } = new List<IntegrationPointTypeTest>();

		//public IList<JobHistoryTest> JobHistory { get; } = new List<JobHistoryTest>();
		private IList<JobHistoryTest> _jobHistory = new List<JobHistoryTest>();

		public IList<JobHistoryTest> JobHistory
		{
			get
			{
				return _jobHistory;
			}
		}

		public IList<JobHistoryErrorTest> JobHistoryErrors { get; } = new List<JobHistoryErrorTest>();

        public IList<SourceProviderTest> SourceProviders { get; } = new List<SourceProviderTest>();

        public IList<DestinationProviderTest> DestinationProviders { get; } = new List<DestinationProviderTest>();

        public IList<FolderTest> Folders { get; } = new List<FolderTest>();

        public IList<SyncConfigurationTest> SyncConfigurations { get; } = new List<SyncConfigurationTest>();

        public IList<SavedSearchTest> SavedSearches { get; } = new List<SavedSearchTest>();

        public IList<FieldTest> Fields { get; } = new List<FieldTest>();

        public IList<ArtifactTest> Artifacts => GetAllArtifacts();

        private IList<ArtifactTest> GetAllArtifacts()
        {
            IEnumerable<ArtifactTest> GetArtifacts(IEnumerable<RdoTestBase> rdos) => rdos.Select(x => x.Artifact);

            return GetArtifacts(IntegrationPoints)
                .Concat(GetArtifacts(IntegrationPointTypes))
                .Concat(GetArtifacts(JobHistory))
                .Concat(GetArtifacts(JobHistoryErrors))
                .Concat(GetArtifacts(SourceProviders))
                .Concat(GetArtifacts(DestinationProviders))
                .Concat(GetArtifacts(Folders))
                .Concat(GetArtifacts(SavedSearches))
                .Concat(GetArtifacts(SyncConfigurations))
                .Concat(GetArtifacts(Fields))
                .ToList();
        }
        
        public IWorkspaceHelpers Helpers { get; }

        public WorkspaceTest(ISerializer serializer, int? workspaceArtifactId = null) : base("Workspace", workspaceArtifactId)
        {
            Name = $"Workspace - {Guid.NewGuid()}";

            FileShareServer = new FileShareResourceServer
            {
                UNCPath = $@"\\emttest\DefaultFileRepository"
            };

            Helpers = new WorkspaceHelpers(this, serializer);
        }

        /// <summary>
        /// Used to realize ObjectManager.Read, which does not specify object type
        /// </summary>
        public RdoTestBase ReadArtifact(int artifactId)
        {
            RdoTestBase TryFind<T>(IList<T> rdos) where T : RdoTestBase
            {
                return rdos.FirstOrDefault(x => x.ArtifactId == artifactId);
            }

            return TryFind(IntegrationPoints)
                   ?? TryFind(IntegrationPointTypes)
                   ?? TryFind(JobHistory)
                   ?? TryFind(JobHistoryErrors)
                   ?? TryFind(SourceProviders)
                   ?? TryFind(DestinationProviders)
                   ?? TryFind(Folders)
                   ?? TryFind(SavedSearches)
                   ?? TryFind(Fields)
                   ?? TryFind(SyncConfigurations);
        }

        public override List<Guid> Guids => new List<Guid>();

        public override RelativityObject ToRelativityObject()
        {
            return new RelativityObject
            {
                ArtifactID = ArtifactId,
                FieldValues = new List<FieldValuePair>
                {
                    new FieldValuePair
                    {
                        Field = new Field
                        {
                            Name = WorkspaceFieldsConstants.NAME_FIELD,
                        },
                        Value = Name
                    }
                },
            };
        }

        private class WorkspaceHelpers : IWorkspaceHelpers
        {
            private readonly WorkspaceTest _workspace;
            private DestinationProviderHelper _destinationProviderHelper;
            private SourceProviderHelper _sourceProviderHelper;
            private ISerializer _serializer;
            private IntegrationPointHelper _integrationPointHelper;
            private IntegrationPointTypeHelper _integrationPointTypeHelper;
            private JobHistoryHelper _jobHistoryHelper;
            private FieldsMappingHelper _fieldsMappingHelper;

            internal WorkspaceHelpers(WorkspaceTest workspace, ISerializer serializer)
            {
                _workspace = workspace;
                _serializer = serializer;
            }
            
            public DestinationProviderHelper DestinationProviderHelper => _destinationProviderHelper ??
                                                                          (_destinationProviderHelper =
                                                                              new DestinationProviderHelper(
                                                                                  _workspace));

            public SourceProviderHelper SourceProviderHelper => _sourceProviderHelper ??
                                                                (_sourceProviderHelper =
                                                                    new SourceProviderHelper(_workspace));

            public IntegrationPointHelper IntegrationPointHelper => _integrationPointHelper ??
                                                                    (_integrationPointHelper =
                                                                        new IntegrationPointHelper(_workspace,
                                                                            _serializer));
            
            public IntegrationPointTypeHelper IntegrationPointTypeHelper => _integrationPointTypeHelper ??
                                                                    (_integrationPointTypeHelper =
                                                                        new IntegrationPointTypeHelper(_workspace));
            public JobHistoryHelper JobHistoryHelper => _jobHistoryHelper ??
                                                        (_jobHistoryHelper =
                                                            new JobHistoryHelper(_workspace));

            public FieldsMappingHelper FieldsMappingHelper => _fieldsMappingHelper ??
                                                              (_fieldsMappingHelper = 
                                                                  new FieldsMappingHelper(_workspace));
        }
    }
}