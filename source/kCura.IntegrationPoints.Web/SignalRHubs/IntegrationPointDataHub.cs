using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
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
        private ISessionService _sessionService;
        private IIntegrationPointManager _integrationPointManager;
	    private IStateManager _stateManager;

        public IntegrationPointDataHub() :
            this(new ContextContainer(ConnectionHelper.Helper()), new ManagerFactory(), new HelperClassFactory())
        {
        }

        public IntegrationPointDataHub(IContextContainer context, IManagerFactory managerFactory, IHelperClassFactory helperClassFactory)
        {
            _context = context;
            _managerFactory = managerFactory;
	        _helperClassFactory = helperClassFactory;
			_integrationPointManager = _managerFactory.CreateIntegrationPointManager(_context);
	        _stateManager = _managerFactory.CreateStateManager(_context);

            if (_tasks == null) _tasks = new SortedDictionary<string, IntegrationPointDataHubInput>();

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
						bool sourceProviderIsRelativity = _integrationPointManager.IntegrationPointSourceProviderIsRelativity(input.WorkspaceId, integrationPointDTO);
						PermissionCheckDTO permissionCheck = _integrationPointManager.UserHasPermissions(input.WorkspaceId, integrationPointDTO, sourceProviderIsRelativity);

						if (integrationPointDTO != null)
                        {
                            IntegrationModel model = new IntegrationModel()
                            {
                                LastRun = integrationPointDTO.LastRuntimeUTC,
                                NextRun = integrationPointDTO.NextScheduledRuntimeUTC
                            };

	                        var buttonStates = new ButtonStateDTO();
							var onClickEvents = new OnClickEventDTO();
							if (sourceProviderIsRelativity)
							{
								IOnClickEventHelper onClickEventHelper = _helperClassFactory.CreateOnClickEventHelper(_managerFactory, _context);

		                        buttonStates = _stateManager.GetButtonState(input.WorkspaceId, input.ArtifactId, permissionCheck.Success,
			                        integrationPointHasErrors);
								onClickEvents = onClickEventHelper.GetOnClickEventsForRelativityProvider(input.WorkspaceId, input.ArtifactId, buttonStates);

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