using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Data;
using Relativity.IntegrationPoints.Tests.Integration.Helpers;
using Relativity.IntegrationPoints.Tests.Integration.Helpers.WorkspaceHelpers;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
    public class WorkspaceTest : IDisposable
    {
        private readonly ProxyMock _proxy;

        public int ArtifactId { get; set; }
        public string Name { get; set; }

        private readonly ObservableCollection<IntegrationPointTest> _integrationPoints =
            new ObservableCollection<IntegrationPointTest>();

        private readonly ObservableCollection<IntegrationPointTypeTest> _integrationPointTypes =
            new ObservableCollection<IntegrationPointTypeTest>();

        private readonly ObservableCollection<JobHistoryTest> _jobHistory = new ObservableCollection<JobHistoryTest>();

        private readonly ObservableCollection<SourceProviderTest> _sourceProviders =
            new ObservableCollection<SourceProviderTest>();

        private readonly ObservableCollection<DestinationProviderTest> _destinationProviders =
            new ObservableCollection<DestinationProviderTest>();

        private readonly ObservableCollection<FolderTest> _folders = new ObservableCollection<FolderTest>();
        private readonly ObservableCollection<ArtifactTest> _artifacts = new ObservableCollection<ArtifactTest>();

        private readonly ObservableCollection<SavedSearchTest> _savedSearches =
            new ObservableCollection<SavedSearchTest>();

        private readonly ObservableCollection<FieldTest> _fields = new ObservableCollection<FieldTest>();
        private readonly CompositeDisposable _cleanup = new CompositeDisposable();

        public IList<IntegrationPointTest> IntegrationPoints => _integrationPoints;

        public IList<IntegrationPointTypeTest> IntegrationPointTypes => _integrationPointTypes;

        public IList<JobHistoryTest> JobHistory => _jobHistory;

        public IList<SourceProviderTest> SourceProviders => _sourceProviders;

        public IList<DestinationProviderTest> DestinationProviders => _destinationProviders;

        public IList<FolderTest> Folders => _folders;

        public IList<ArtifactTest> Artifacts => _artifacts;

        public IList<SavedSearchTest> SavedSearches => _savedSearches;

        public IList<FieldTest> Fields => _fields;
        
        public IWorkspaceHelpers Helpers { get; }

        public WorkspaceTest(ProxyMock proxy, ISerializer serializer, int? workspaceArtifactId = null)
        {
            _proxy = proxy;
            ArtifactId = workspaceArtifactId ?? ArtifactProvider.NextId();
            Name = $"Workspace - {Guid.NewGuid()}";

            Helpers = new WorkspaceHelpers(this, serializer);
            
            SetupIntegrationPoints();
            SetupIntegrationPointTypes();
            SetupJobHistory();
            SetupSourceProviders();
            SetupDestinationProviders();
            SetupFolders();
            SetupSavedSearches();
            SetupFields();
        }

        public RelativityObject ToRelativityObject()
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

        private void SetupIntegrationPoints()
        {
            _integrationPoints.SetupOnAddedHandler((newItem) =>
                {
                    _proxy.ObjectManager.SetupArtifact(this, newItem);
                    _proxy.ObjectManager.SetupIntegrationPoint(this, newItem);
                })
                .DisposeWith(_cleanup);
        }

        private void SetupIntegrationPointTypes()
        {
            _integrationPointTypes
                .SetupOnAddedHandler((newItem) => _proxy.ObjectManager.SetupIntegrationPointType(this, newItem))
                .DisposeWith(_cleanup);
        }

        private void SetupJobHistory()
        {
            _jobHistory.SetupOnAddedHandler(newItem => _proxy.ObjectManager.SetupJobHistory(this, newItem))
                .DisposeWith(_cleanup);
        }

        private void SetupSourceProviders()
        {
            _sourceProviders.SetupOnAddedHandler(newItem => _proxy.ObjectManager.SetupSourceProvider(this, newItem))
                .DisposeWith(_cleanup);
        }

        private void SetupDestinationProviders()
        {
            _destinationProviders
                .SetupOnAddedHandler(newItem => _proxy.ObjectManager.SetupDestinationProvider(this, newItem))
                .DisposeWith(_cleanup);
        }

        private void SetupFolders()
        {
            _folders.SetupOnAddedHandler(newItem =>
                {
                    this.Artifacts.Add(newItem.Artifact);
                    _proxy.ObjectManager.SetupArtifact(this, newItem);
                })
                .DisposeWith(_cleanup);
        }

        private void SetupSavedSearches()
        {
            _savedSearches.SetupOnAddedHandler(newItem => { _proxy.ObjectManager.SetupSavedSearch(this, newItem); })
                .DisposeWith(_cleanup);
        }

        private void SetupFields()
        {
            _destinationProviders.SetupOnAddedHandler(newItem =>
                {
                    //TODO
                })
                .DisposeWith(_cleanup);
        }

        public void Dispose()
        {
            _cleanup.Dispose();
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

            public FieldsMappingHelper FieldsMappingHelper =>
                _fieldsMappingHelper ?? (_fieldsMappingHelper = new FieldsMappingHelper(_workspace));
        }
    }
}