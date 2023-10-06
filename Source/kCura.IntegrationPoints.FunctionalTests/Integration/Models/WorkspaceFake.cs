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
    /// <inheritdoc />
    public class WorkspaceFake : RdoFakeBase
    {
        public string Name { get; set; }

        public FileShareResourceServer FileShareServer { get; set; }

        public IList<IntegrationPointFake> IntegrationPoints { get; } = new List<IntegrationPointFake>();

        public IList<IntegrationPointProfileFake> IntegrationPointProfiles { get; } = new List<IntegrationPointProfileFake>();

        public IList<IntegrationPointTypeFake> IntegrationPointTypes { get; } = new List<IntegrationPointTypeFake>();

        public IList<JobHistoryFake> JobHistory
        {
            get;
        } = new List<JobHistoryFake>();

        public IList<JobHistoryErrorFake> JobHistoryErrors { get; } = new List<JobHistoryErrorFake>();

        public IList<SourceProviderFake> SourceProviders { get; } = new List<SourceProviderFake>();

        public IList<DestinationProviderFake> DestinationProviders { get; } = new List<DestinationProviderFake>();

        public IList<FolderFake> Folders { get; } = new List<FolderFake>();

        public IList<SyncConfigurationFake> SyncConfigurations { get; } = new List<SyncConfigurationFake>();

        public IList<SavedSearchFake> SavedSearches { get; } = new List<SavedSearchFake>();

        public IList<ViewFake> Views { get; } = new List<ViewFake>();

        public IList<FieldFake> Fields { get; } = new List<FieldFake>();

        public IList<ObjectTypeFake> ObjectTypes { get; } = new List<ObjectTypeFake>();

        public IList<EntityFake> Entities { get; } = new List<EntityFake>();

        public IList<DocumentFake> Documents { get; } = new List<DocumentFake>();

        public IList<ProductionFake> Productions { get; } = new List<ProductionFake>();

        public IList<ArtifactTest> Artifacts => GetAllArtifacts();
        private IList<ArtifactTest> GetAllArtifacts()
        {
            IEnumerable<ArtifactTest> GetArtifacts(IEnumerable<RdoFakeBase> rdos) => rdos.Select(x => x.Artifact);

            return GetArtifacts(IntegrationPoints)
                .Concat(GetArtifacts(IntegrationPointTypes))
                .Concat(GetArtifacts(IntegrationPointProfiles))
                .Concat(GetArtifacts(JobHistory))
                .Concat(GetArtifacts(JobHistoryErrors))
                .Concat(GetArtifacts(SourceProviders))
                .Concat(GetArtifacts(DestinationProviders))
                .Concat(GetArtifacts(Folders))
                .Concat(GetArtifacts(SavedSearches))
                .Concat(GetArtifacts(Views))
                .Concat(GetArtifacts(SyncConfigurations))
                .Concat(GetArtifacts(ObjectTypes))
                .Concat(GetArtifacts(Fields))
                .Concat(GetArtifacts(Documents))
                .Concat(GetArtifacts(Productions))
                .ToList();
        }

        public IWorkspaceHelpers Helpers { get; }

        public WorkspaceFake(ISerializer serializer, int? workspaceArtifactId = null) : base("Workspace", workspaceArtifactId)
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
        public RdoFakeBase ReadArtifact(int artifactId)
        {
            RdoFakeBase TryFind<T>(IList<T> rdos) where T : RdoFakeBase
            {
                return rdos.FirstOrDefault(x => x.ArtifactId == artifactId);
            }

            return TryFind(IntegrationPoints)
                   ?? TryFind(IntegrationPointProfiles)
                   ?? TryFind(IntegrationPointTypes)
                   ?? TryFind(JobHistory)
                   ?? TryFind(JobHistoryErrors)
                   ?? TryFind(SourceProviders)
                   ?? TryFind(DestinationProviders)
                   ?? TryFind(Folders)
                   ?? TryFind(SavedSearches)
                   ?? TryFind(Views)
                   ?? TryFind(Fields)
                   ?? TryFind(SyncConfigurations)
                   ?? TryFind(Documents)
                   ?? TryFind(Productions);
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
            private readonly WorkspaceFake _workspace;
            private DestinationProviderHelper _destinationProviderHelper;
            private SourceProviderHelper _sourceProviderHelper;
            private ISerializer _serializer;
            private IntegrationPointHelper _integrationPointHelper;
            private IntegrationPointProfileHelper _integrationPointProfileHelper;
            private IntegrationPointTypeHelper _integrationPointTypeHelper;
            private JobHistoryHelper _jobHistoryHelper;
            private FieldsMappingHelper _fieldsMappingHelper;
            private DocumentHelper _documentHelper;
            private ProductionHelper _productionHelper;
            private SavedSearchHelper _savedSearchHelper;

            internal WorkspaceHelpers(WorkspaceFake workspace, ISerializer serializer)
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
            public IntegrationPointProfileHelper IntegrationPointProfileHelper => _integrationPointProfileHelper ??
                                                                    (_integrationPointProfileHelper =
                                                                        new IntegrationPointProfileHelper(_workspace,
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

            public DocumentHelper DocumentHelper => _documentHelper ??
                                                    (_documentHelper =
                                                        new DocumentHelper(_workspace));

            public ProductionHelper ProductionHelper => _productionHelper ??
                                                      (_productionHelper =
                                                          new ProductionHelper(_workspace));

            public SavedSearchHelper SavedSearchHelper => _savedSearchHelper ??
                                                        (_savedSearchHelper =
                                                            new SavedSearchHelper(_workspace));
        }
    }
}
