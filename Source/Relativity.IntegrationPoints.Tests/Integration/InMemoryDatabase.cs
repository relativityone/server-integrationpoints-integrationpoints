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

		public List<AgentTest> Agents { get; set; } = new List<AgentTest>();

		public List<JobTest> JobsInQueue { get; set; } = new List<JobTest>();

		public IList<WorkspaceTest> Workspaces => _workspaces;

		public IList<IntegrationPointTest> IntegrationPoints => _integrationPoints;

		public IList<IntegrationPointTypeTest> IntegrationPointTypes => _integrationPointTypes;

		public IList<JobHistoryTest> JobHistory => _jobHistory;

		public IList<SourceProviderTest> SourceProviders => _sourceProviders;

		public IList<DestinationProviderTest> DestinationProviders => _destinationProviders;

		public InMemoryDatabase(ProxyMock proxy)
		{
			_proxy = proxy;

			SetupWorkspaces();
			SetupIntegrationPoints();
			SetupIntegrationPointTypes();
			SetupJobHistory();
			SetupSourceProviders();
			SetupDestinationProviders();
		}

		public void Clear()
		{
			Agents.Clear();
			JobsInQueue.Clear();
			Workspaces.Clear();
			IntegrationPoints.Clear();
			IntegrationPointTypes.Clear();
			JobHistory.Clear();
			SourceProviders.Clear();
			DestinationProviders.Clear();
		}

		private void SetupWorkspaces()
		{
			_workspaces.CollectionChanged += (sender, args) =>
			{
				OnNewItemsAdded<WorkspaceTest>(sender, args,
					(newItems) => _proxy.ObjectManager.SetupWorkspace(this, newItems));
			};
		}

		private void SetupIntegrationPoints()
		{
			_integrationPoints.CollectionChanged += (sender, args) =>
			{
				OnNewItemsAdded<IntegrationPointTest>(sender, args,
					(newItems) => _proxy.ObjectManager.SetupIntegrationPoints(this, newItems));
			};
		}

		private void SetupIntegrationPointTypes()
		{
			_integrationPointTypes.CollectionChanged += (sender, args) =>
			{
				OnNewItemsAdded<IntegrationPointTypeTest>(sender, args,
					(newItems) => _proxy.ObjectManager.SetupIntegrationPointTypes(this, newItems));
			};
		}

		private void SetupJobHistory()
		{
			_jobHistory.CollectionChanged += (sender, args) =>
			{
				OnNewItemsAdded<JobHistoryTest>(sender, args, 
					newItems => _proxy.ObjectManager.SetupJobHistory(this, newItems));
			};
		}

		private void SetupSourceProviders()
		{
			_sourceProviders.CollectionChanged += (sender, args) =>
			{
				OnNewItemsAdded<SourceProviderTest>(sender, args,
					newItems => _proxy.ObjectManager.SetupSourceProviders(this, newItems));
			};
		}

		private void SetupDestinationProviders()
		{
			_destinationProviders.CollectionChanged += (sender, args) =>
			{
				OnNewItemsAdded<DestinationProviderTest>(sender, args,
					newItems => _proxy.ObjectManager.SetupDestinationProviders(this, newItems));
			};
		}

		void OnNewItemsAdded<T>(object sender, NotifyCollectionChangedEventArgs e, Action<IEnumerable<T>> onAddFunc)
		{
			if (e.Action == NotifyCollectionChangedAction.Add)
			{
				IEnumerable<T> newItem = sender as IEnumerable<T>;
				onAddFunc(newItem);
			}
		}
	}
}
