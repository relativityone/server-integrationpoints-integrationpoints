﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using kCura.Apps.Common.Data;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Authentication;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain.Authentication;
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
		private readonly IContextContainer _contextContainer;
		private readonly IQueueManager _queueManager;
		private readonly IJobHistoryManager _jobHistoryManager;
		private readonly IStateManager _stateManager;
		private readonly IHelperClassFactory _helperClassFactory;
		private readonly IIntegrationPointPermissionValidator _permissionValidator;
		private readonly int _intervalBetweentasks = 100;
		private readonly IManagerFactory _managerFactory;
		private readonly int _updateInterval = 5000;

		public IntegrationPointDataHub() : this(new ContextContainer((IHelper)ConnectionHelper.Helper()), new HelperClassFactory())
		{
			IHelper helper = ConnectionHelper.Helper();
			IAPILog logger = helper.GetLoggerFactory().GetLogger();
			ISqlServiceFactory sqlServiceFactory = new HelperConfigSqlServiceFactory(helper);
			IAuthProvider authProvider = new AuthProvider();
			IAuthTokenGenerator authTokenGenerator = new ClaimsTokenGenerator();
			ICredentialProvider credentialProvider = new TokenCredentialProvider(authProvider, authTokenGenerator, helper);
			IServiceManagerProvider serviceManagerProvider = new ServiceManagerProvider(new ConfigFactory(),
				credentialProvider, new JSONSerializer(),
				new RelativityCoreTokenProvider(), sqlServiceFactory);

			_managerFactory = new ManagerFactory(helper, serviceManagerProvider);
			_queueManager = _managerFactory.CreateQueueManager(_contextContainer);
			_jobHistoryManager = _managerFactory.CreateJobHistoryManager(_contextContainer);
			_stateManager = _managerFactory.CreateStateManager();
			IRepositoryFactory repositoryFactory = new RepositoryFactory(helper, helper.GetServicesManager());
			_permissionValidator = new IntegrationPointPermissionValidator(new[] { new ViewErrorsPermissionValidator(repositoryFactory) }, new IntegrationPointSerializer(logger));
		}

		internal IntegrationPointDataHub(IContextContainer contextContainer, IHelperClassFactory helperClassFactory)
		{
			_contextContainer = contextContainer;
			_helperClassFactory = helperClassFactory;

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

		public override Task OnDisconnected(bool stopCalled)
		{
			RemoveTask();
			return base.OnDisconnected(stopCalled);
		}

		public void GetIntegrationPointUpdate(int workspaceId, int artifactId)
		{
			int userId = ((ICPHelper)_contextContainer.Helper).GetAuthenticationManager().UserInfo.ArtifactID;
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

						var permissionRepository = new PermissionRepository((IHelper)ConnectionHelper.Helper(), input.WorkspaceId);
						IRelativityObjectManager objectManager = CreateObjectManager(ConnectionHelper.Helper(), input.WorkspaceId);
						var _providerTypeService = new ProviderTypeService(objectManager);
						var _buttonStateBuilder = new ButtonStateBuilder(_providerTypeService, _queueManager, _jobHistoryManager, _stateManager, permissionRepository, _permissionValidator, objectManager);

						IntegrationPoint integrationPoint = objectManager.Read<IntegrationPoint>(input.ArtifactId);

						ProviderType providerType = _providerTypeService.GetProviderType(integrationPoint.SourceProvider.Value,
							integrationPoint.DestinationProvider.Value);
						bool sourceProviderIsRelativity = providerType == ProviderType.Relativity;

						IntegrationPointModel model = new IntegrationPointModel
						{
							HasErrors = integrationPoint.HasErrors,
							LastRun = integrationPoint.LastRuntimeUTC,
							NextRun = integrationPoint.NextScheduledRuntimeUTC
						};

						IOnClickEventConstructor onClickEventHelper = _helperClassFactory.CreateOnClickEventHelper(_managerFactory, _contextContainer);

						var buttonStates = _buttonStateBuilder.CreateButtonState(input.WorkspaceId, input.ArtifactId);
						var onClickEvents = onClickEventHelper.GetOnClickEvents(input.WorkspaceId, input.ArtifactId, integrationPoint.Name, buttonStates);

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

		private IRelativityObjectManager CreateObjectManager(ICPHelper helper, int workspaceId)
		{
			return new RelativityObjectManagerFactory(helper).CreateRelativityObjectManager(workspaceId);
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