using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Relativity.API;
using Relativity.CustomPages;
using Timer = System.Timers.Timer;

namespace kCura.IntegrationPoints.Web.SignalRHubs
{
	[HubName("IntegrationPointData")]
	public class IntegrationPointDataHub : Hub
	{
		private static SortedDictionary<string, IntegrationPointDataHubInput> _tasks;
		private static Timer _updateTimer;
		private readonly IButtonStateBuilder _buttonStateBuilder;
		private readonly IContextContainer _contextContainer;
		private readonly IHelperClassFactory _helperClassFactory;
		private readonly IIntegrationPointManager _integrationPointManager;
		private readonly int _intervalBetweentasks = 100;
		private readonly IManagerFactory _managerFactory;
		private readonly int _updateInterval = 5000;

		public IntegrationPointDataHub() : this(new ContextContainer(ConnectionHelper.Helper()), new HelperClassFactory(), new ManagerFactory(ConnectionHelper.Helper()))
		{
			var permissionRepository = new PermissionRepository(ConnectionHelper.Helper(), ConnectionHelper.Helper().GetActiveCaseID());
			var queueManager = _managerFactory.CreateQueueManager(_contextContainer);
			var jobHistoryManager = _managerFactory.CreateJobHistoryManager(_contextContainer);
			var stateManager = _managerFactory.CreateStateManager();

			_buttonStateBuilder = new ButtonStateBuilder(_integrationPointManager, queueManager, jobHistoryManager, stateManager, permissionRepository);
		}

		internal IntegrationPointDataHub(IContextContainer contextContainer, IHelperClassFactory helperClassFactory,
			IManagerFactory managerFactory)
		{
			_contextContainer = contextContainer;
			_helperClassFactory = helperClassFactory;
			_managerFactory = managerFactory;
			_integrationPointManager = managerFactory.CreateIntegrationPointManager(_contextContainer);

			if (_tasks == null)
			{
				_tasks = new SortedDictionary<string, IntegrationPointDataHubInput>();
			}

			if (_updateTimer == null)
			{
				_updateTimer = new Timer(_updateInterval);
				_updateTimer.Elapsed += _updateTimer_Elapsed;
				_updateTimer.Start();
			}
		}

		internal IntegrationPointDataHub(IButtonStateBuilder buttonStateBuilder, IContextContainer contextContainer, IHelperClassFactory helperClassFactory,
			IManagerFactory managerFactory) : this(contextContainer, helperClassFactory, managerFactory)
		{
			_buttonStateBuilder = buttonStateBuilder;
		}

		public override Task OnDisconnected(bool stopCalled)
		{
			RemoveTask();
			return base.OnDisconnected(stopCalled);
		}

		public void GetIntegrationPointUpdate(int workspaceId, int artifactId)
		{
			int userId = ((ICPHelper) _contextContainer.Helper).GetAuthenticationManager().UserInfo.ArtifactID;
			AddTask(workspaceId, artifactId, userId);
		}

		private void _updateTimer_Elapsed(object sender, ElapsedEventArgs e)
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

						Core.Constants.SourceProvider sourceProvider = _integrationPointManager.GetSourceProvider(input.WorkspaceId, integrationPointDTO);
						bool sourceProviderIsRelativity = sourceProvider == Core.Constants.SourceProvider.Relativity;

						IntegrationPointModel model = new IntegrationPointModel
						{
							HasErrors = integrationPointDTO.HasErrors,
							LastRun = integrationPointDTO.LastRuntimeUTC,
							NextRun = integrationPointDTO.NextScheduledRuntimeUTC
						};

						IOnClickEventConstructor onClickEventHelper = _helperClassFactory.CreateOnClickEventHelper(_managerFactory, _contextContainer);

						var buttonStates = _buttonStateBuilder.CreateButtonState(input.WorkspaceId, input.ArtifactId);
						var onClickEvents = onClickEventHelper.GetOnClickEvents(input.WorkspaceId, input.ArtifactId, buttonStates);

						Clients.Group(key).updateIntegrationPointData(model, buttonStates, onClickEvents, sourceProviderIsRelativity);

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
						{
							_tasks[key].ConnectionIds.Remove(Context.ConnectionId);
						}
						if (_tasks[key].ConnectionIds.Count == 0)
						{
							_tasks.Remove(key);
						}
					}
				}
			}
		}

		private string GetKey(int workspaceId, int artifactId, int userId)
		{
			return $"{userId}{workspaceId}{artifactId}";
		}
	}
}