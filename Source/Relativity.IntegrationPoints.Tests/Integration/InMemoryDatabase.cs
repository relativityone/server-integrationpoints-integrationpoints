using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration
{
	public class InMemoryDatabase
	{
		private readonly ProxyMock _proxy;
		private readonly ObservableCollection<WorkspaceTest> _workspaces = new ObservableCollection<WorkspaceTest>();
		private readonly ObservableCollection<IntegrationPointTest> _integrationPoints = new ObservableCollection<IntegrationPointTest>();
		private readonly ObservableCollection<IntegrationPointTypeTest> _integrationPointTypes = new ObservableCollection<IntegrationPointTypeTest>();
		private readonly ObservableCollection<JobHistoryTest> _jobHistory = new ObservableCollection<JobHistoryTest>();
		private readonly ObservableCollection<SourceProviderTest> _sourceProviders = new ObservableCollection<SourceProviderTest>();
		private readonly ObservableCollection<DestinationProviderTest> _destinationProviders = new ObservableCollection<DestinationProviderTest>();
		private readonly ObservableCollection<FolderTest> _folders = new ObservableCollection<FolderTest>();
		private readonly ObservableCollection<ArtifactTest> _artifacts = new ObservableCollection<ArtifactTest>();
		private readonly ObservableCollection<SavedSearchTest> _savedSearches = new ObservableCollection<SavedSearchTest>();
		private readonly ObservableCollection<FieldTest> _fields = new ObservableCollection<FieldTest>();

		public List<AgentTest> Agents { get; set; } = new List<AgentTest>();

		public List<JobTest> JobsInQueue { get; set; } = new List<JobTest>();

		public IList<WorkspaceTest> Workspaces => _workspaces;

		public IList<IntegrationPointTest> IntegrationPoints => _integrationPoints;

		public IList<IntegrationPointTypeTest> IntegrationPointTypes => _integrationPointTypes;

		public IList<JobHistoryTest> JobHistory => _jobHistory;

		public IList<SourceProviderTest> SourceProviders => _sourceProviders;

		public IList<DestinationProviderTest> DestinationProviders => _destinationProviders;

		public IList<FolderTest> Folders => _folders;

		public IList<ArtifactTest> Artifacts => _artifacts;

		public IList<SavedSearchTest> SavedSearches => _savedSearches;

		public IList<FieldTest> Fields => _fields;

		public InMemoryDatabase(ProxyMock proxy)
		{
			_proxy = proxy;

			SetupWorkspaces();
			SetupFolders();
			SetupIntegrationPoints();
			SetupIntegrationPointTypes();
			SetupJobHistory();
			SetupSourceProviders();
			SetupDestinationProviders();
			SetupSavedSearches();
			SetupFields();
		}

		public void Clear()
		{
			Agents.Clear();
			JobsInQueue.Clear();
			Workspaces.Clear();
			Folders.Clear();
			IntegrationPoints.Clear();
			IntegrationPointTypes.Clear();
			JobHistory.Clear();
			SourceProviders.Clear();
			DestinationProviders.Clear();
			SavedSearches.Clear();
			Fields.Clear();
		}

		private void SetupWorkspaces()
		{
			_workspaces.CollectionChanged += (sender, args) =>
			{
				OnNewItemsAdded<WorkspaceTest>(sender, args,
					(newItem) =>
					{
						_proxy.ObjectManager.SetupDocumentFields(this, newItem);
						_proxy.ObjectManager.SetupWorkspace(this, newItem);
					});
			};
		}

		private void SetupIntegrationPoints()
		{
			_integrationPoints.CollectionChanged += (sender, args) =>
			{
				OnNewItemsAdded<IntegrationPointTest>(sender, args,
					(newItem) =>
					{
						_proxy.ObjectManager.SetupArtifact(this, newItem);
						_proxy.ObjectManager.SetupIntegrationPoint(this, newItem);
					});
			};
		}

		private void SetupIntegrationPointTypes()
		{
			_integrationPointTypes.CollectionChanged += (sender, args) =>
			{
				OnNewItemsAdded<IntegrationPointTypeTest>(sender, args,
					(newItem) => _proxy.ObjectManager.SetupIntegrationPointType(this, newItem));
			};
		}

		private void SetupJobHistory()
		{
			_jobHistory.CollectionChanged += (sender, args) =>
			{
				OnNewItemsAdded<JobHistoryTest>(sender, args, 
					newItem => _proxy.ObjectManager.SetupJobHistory(this, newItem));
			};
		}

		private void SetupSourceProviders()
		{
			_sourceProviders.CollectionChanged += (sender, args) =>
			{
				OnNewItemsAdded<SourceProviderTest>(sender, args,
					newItem => _proxy.ObjectManager.SetupSourceProvider(this, newItem));
			};
		}

		private void SetupDestinationProviders()
		{
			_destinationProviders.CollectionChanged += (sender, args) =>
			{
				OnNewItemsAdded<DestinationProviderTest>(sender, args,
					newItem => _proxy.ObjectManager.SetupDestinationProvider(this, newItem));
			};
		}

		private void SetupFolders()
		{
			_folders.CollectionChanged += (sender, args) =>
			{
				OnNewItemsAdded<FolderTest>(sender, args,
					newItem =>
					{
						this.Artifacts.Add(newItem.Artifact);
						_proxy.ObjectManager.SetupArtifact(this, newItem);
					});
			};
		}

		private void SetupSavedSearches()
		{
			_savedSearches.CollectionChanged += (sender, args) =>
			{
				OnNewItemsAdded<SavedSearchTest>(sender, args,
					newItem =>
					{
						_proxy.ObjectManager.SetupSavedSearch(this, newItem);
					});
			};
		}

		private void SetupFields()
		{
			_fields.CollectionChanged += (sender, args) =>
			{
				OnNewItemsAdded<FieldTest>(sender, args,
					newItem =>
					{
						//TODO
					});
			};
		}

		void OnNewItemsAdded<T>(object sender, NotifyCollectionChangedEventArgs e, Action<T> onAddFunc)
		{
			if (e.Action == NotifyCollectionChangedAction.Add)
			{
				foreach (T newItem in e.NewItems)
				{
					onAddFunc(newItem);
				}
			}
		}
	}
}
