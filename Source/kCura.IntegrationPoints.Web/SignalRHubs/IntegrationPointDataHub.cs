using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Domain.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Relativity.API;
using Relativity.CustomPages;

namespace kCura.IntegrationPoints.Web.SignalRHubs
{
	[HubName("IntegrationPointData")]
	public class IntegrationPointDataHub : Hub
	{
		private static SortedDictionary<string, IntegrationPointDataHubInput> _tasks;
		private static System.Timers.Timer _updateTimer;
		private int _updateInterval = 5000;
		private int _intervalBetweentasks = 100;
		private IContextContainer _context;
		private IManagerFactory _managerFactory;
		private IHelperClassFactory _helperClassFactory;
		private IIntegrationPointManager _integrationPointManager;
		private IJobHistoryManager _jobHistoryManager;
		private IQueueManager _queueManager;
		private IStateManager _stateManager;

		public IntegrationPointDataHub() :
			this(new ContextContainer(ConnectionHelper.Helper()), new ManagerFactory(ConnectionHelper.Helper()), new HelperClassFactory())
		{
		}

		internal IntegrationPointDataHub(IContextContainer context, IManagerFactory managerFactory, IHelperClassFactory helperClassFactory)
		{
			_context = context;
			_managerFactory = managerFactory;
			_helperClassFactory = helperClassFactory;
			_integrationPointManager = _managerFactory.CreateIntegrationPointManager(_context);
			_jobHistoryManager = _managerFactory.CreateJobHistoryManager(_context);
			_queueManager = _managerFactory.CreateQueueManager(_context);
			_stateManager = _managerFactory.CreateStateManager();

			if (_tasks == null)
			{
				_tasks = new SortedDictionary<string, IntegrationPointDataHubInput>();
			}

			if (_updateTimer == null)
			{
				_updateTimer = new System.Timers.Timer(_updateInterval);
				_updateTimer.Elapsed += _updateTimer_Elapsed;
				_updateTimer.Start();
			}
		}

		public override System.Threading.Tasks.Task OnConnected()
		{
			return base.OnConnected();
		}

		public override System.Threading.Tasks.Task OnReconnected()
		{
			return base.OnReconnected();
		}

		public override Task OnDisconnected(bool stopCalled)
		{
			RemoveTask();
			return base.OnDisconnected(stopCalled);
		}

		public void GetIntegrationPointUpdate(int workspaceId, int artifactId)
		{
			int userId = ((ICPHelper)_context.Helper).GetAuthenticationManager().UserInfo.ArtifactID;
			AddTask(workspaceId, artifactId, userId);
		}

		private void _updateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			try
			{
				_updateTimer.Enabled = false;
				lock (_tasks)
				{
					foreach (var key in _tasks.Keys)
					{
						IntegrationPointDataHubInput input = _tasks[key];
						IntegrationPointDTO integrationPointDTO = _integrationPointManager.Read(input.WorkspaceId, input.ArtifactId);
						bool integrationPointHasErrors = integrationPointDTO.HasErrors.GetValueOrDefault(false);

						Core.Constants.SourceProvider sourceProvider = _integrationPointManager.GetSourceProvider(input.WorkspaceId, integrationPointDTO);
						bool sourceProviderIsRelativity = (sourceProvider == Core.Constants.SourceProvider.Relativity);

						if (integrationPointDTO != null)
						{
							IntegrationModel model = new IntegrationModel()
							{
								HasErrors = integrationPointDTO.HasErrors,
								LastRun = integrationPointDTO.LastRuntimeUTC,
								NextRun = integrationPointDTO.NextScheduledRuntimeUTC
							};

							var buttonStates = new ButtonStateDTO();
							var onClickEvents = new OnClickEventDTO();
							IOnClickEventConstructor onClickEventHelper = _helperClassFactory.CreateOnClickEventHelper(_managerFactory, _context);
							StoppableJobCollection stoppableJobCollection = _jobHistoryManager.GetStoppableJobCollection(input.WorkspaceId, input.ArtifactId);
							bool hasStoppableJobs = stoppableJobCollection.HasStoppableJobs;
							bool hasJobsExecutingOrInQueue = _queueManager.HasJobsExecutingOrInQueue(input.WorkspaceId, input.ArtifactId);
							if (sourceProviderIsRelativity)
							{
								// NOTE: we are always passing true for now. Once we figure out why the ExecutionIdentity.CurrentUser isn't always the same -- biedrzycki: May 25th, 2016
								buttonStates = _stateManager.GetRelativityProviderButtonState(hasJobsExecutingOrInQueue, integrationPointHasErrors, true, hasStoppableJobs);
								onClickEvents = onClickEventHelper.GetOnClickEventsForRelativityProvider(input.WorkspaceId, input.ArtifactId,
									(RelativityButtonStateDTO)buttonStates);
							}
							else
							{
								buttonStates = _stateManager.GetButtonState(hasJobsExecutingOrInQueue, hasStoppableJobs);
								onClickEvents = onClickEventHelper.GetOnClickEvents(input.WorkspaceId, input.ArtifactId, buttonStates);
							}

							Clients.Group(key).updateIntegrationPointData(model, buttonStates, onClickEvents, sourceProviderIsRelativity);
						}

						//sleep between getting each stats to get SQL Server a break
						Thread.Sleep(_intervalBetweentasks);
					}
				}
			}
			finally
			{
				_updateTimer.Enabled = true;
			}
		}

		public void AddTask(int workspaceId, int artifactId, int userId)
		{
			string key = GetKey(workspaceId, artifactId, userId);
			lock (_tasks)
			{
				Groups.Add(Context.ConnectionId, key);
				if (!_tasks.ContainsKey(key))
				{
					_tasks.Add(key, new IntegrationPointDataHubInput(workspaceId, artifactId, userId, Context.ConnectionId));
				}
				else
				{
					if (!_tasks[key].ConnectionIds.Contains(Context.ConnectionId))
					{
						_tasks[key].ConnectionIds.Add(Context.ConnectionId);
					}
				}
			}
		}

		private void RemoveTask()
		{
			lock (_tasks)
			{
				string key = _tasks.Values.Where(x => x.ConnectionIds.Contains(Context.ConnectionId)).Select(x => GetKey(x.WorkspaceId, x.ArtifactId, x.UserId)).FirstOrDefault();
				if (!string.IsNullOrEmpty(key))
				{
					Groups.Remove(Context.ConnectionId, key);
					if (_tasks.ContainsKey(key))
					{
						if (_tasks[key].ConnectionIds.Contains(Context.ConnectionId))
							_tasks[key].ConnectionIds.Remove(Context.ConnectionId);
						if (_tasks[key].ConnectionIds.Count == 0) _tasks.Remove(key);
					}
				}
			}
		}

		private string GetKey(int workspaceId, int artifactId, int userId)
		{
			return string.Format("{0}{1}{2}", userId, workspaceId, artifactId);
		}
	}
}