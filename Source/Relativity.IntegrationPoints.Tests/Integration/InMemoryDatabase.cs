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

		public ObservableCollection<JobHistoryTest> JobHistory { get; set; } = new ObservableCollection<JobHistoryTest>();

		public InMemoryDatabase(ProxyMock proxy)
		{
			_proxy = proxy;

			SetupWorkspaces();
			SetupIntegrationPoints();
			SetupJobHistory();
		}

		public void Clear()
		{
			Agents.Clear();
			JobsInQueue.Clear();
			Workspaces.Clear();
			IntegrationPoints.Clear();
			JobHistory.Clear();
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

		private void SetupJobHistory()
		{
			JobHistory.CollectionChanged += (sender, args) =>
			{
				OnNewItemsAdded<JobHistoryTest>(sender, args, 
					newItems => _proxy.ObjectManager.SetupJobHistory(this, newItems));
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
