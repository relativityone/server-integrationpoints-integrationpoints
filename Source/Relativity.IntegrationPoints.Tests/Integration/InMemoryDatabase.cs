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

		public List<AgentTest> Agents { get; set; } = new List<AgentTest>();

		public List<JobTest> JobsInQueue { get; set; } = new List<JobTest>();

		public ObservableCollection<WorkspaceTest> Workspaces { get; set; } = new ObservableCollection<WorkspaceTest>();

		public ObservableCollection<IntegrationPointTest> IntegrationPoints { get; set; } = new ObservableCollection<IntegrationPointTest>();

		public ObservableCollection<IntegrationPointTypeTest> IntegrationPointTypes { get; set; } = new ObservableCollection<IntegrationPointTypeTest>();

		public ObservableCollection<JobHistoryTest> JobHistory { get; set; } = new ObservableCollection<JobHistoryTest>();

		public ObservableCollection<SourceProviderTest> SourceProviders { get; set; } = new ObservableCollection<SourceProviderTest>();

		public ObservableCollection<DestinationProviderTest> DestinationProviders { get; set; } = new ObservableCollection<DestinationProviderTest>();

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
			Workspaces.CollectionChanged += (sender, args) =>
			{
				OnNewItemsAdded<WorkspaceTest>(sender, args,
					(newItems) => _proxy.ObjectManager.SetupWorkspace(this, newItems));
			};
		}

		private void SetupIntegrationPoints()
		{
			IntegrationPoints.CollectionChanged += (sender, args) =>
			{
				OnNewItemsAdded<IntegrationPointTest>(sender, args,
					(newItems) => _proxy.ObjectManager.SetupIntegrationPoints(this, newItems));
			};
		}

		private void SetupIntegrationPointTypes()
		{
			IntegrationPointTypes.CollectionChanged += (sender, args) =>
			{
				OnNewItemsAdded<IntegrationPointTypeTest>(sender, args,
					(newItems) => _proxy.ObjectManager.SetupIntegrationPointTypes(this, newItems));
			};
		}

		private void SetupJobHistory()
		{
			JobHistory.CollectionChanged += (sender, args) =>
			{
				OnNewItemsAdded<JobHistoryTest>(sender, args, 
					newItems => _proxy.ObjectManager.SetupJobHistory(this, newItems));
			};
		}

		private void SetupSourceProviders()
		{
			SourceProviders.CollectionChanged += (sender, args) =>
			{
				OnNewItemsAdded<SourceProviderTest>(sender, args,
					newItems => _proxy.ObjectManager.SetupSourceProviders(this, newItems));
			};
		}

		private void SetupDestinationProviders()
		{
			DestinationProviders.CollectionChanged += (sender, args) =>
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
