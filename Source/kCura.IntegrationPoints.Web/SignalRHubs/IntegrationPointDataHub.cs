using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
		private static ConcurrentDictionary<IntegrationPointDataHubKey, HashSet<string>> _tasks;
		private static Timer _updateTimer;

		private readonly IAPILog _logger;
		private readonly IContextContainer _contextContainer;
		private readonly IHelperClassFactory _helperClassFactory;
		private readonly IIntegrationPointPermissionValidator _permissionValidator;
		private readonly IJobHistoryManager _jobHistoryManager;
		private readonly IManagerFactory _managerFactory;
		private readonly int _intervalBetweenTasks = 100;
		private readonly int _updateInterval = 5000;
		private readonly IQueueManager _queueManager;
		private readonly IStateManager _stateManager;

		public IntegrationPointDataHub() : this(new ContextContainer((IHelper)ConnectionHelper.Helper()), new HelperClassFactory())
		{
			IHelper helper = ConnectionHelper.Helper();
			_logger = helper.GetLoggerFactory().GetLogger();
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
			_permissionValidator = new IntegrationPointPermissionValidator(new[] { new ViewErrorsPermissionValidator(repositoryFactory) }, new IntegrationPointSerializer(_logger));
		}

		internal IntegrationPointDataHub(IContextContainer contextContainer, IHelperClassFactory helperClassFactory)
		{
			_contextContainer = contextContainer;
			_helperClassFactory = helperClassFactory;

			if (_tasks == null)
			{
				_tasks = new ConcurrentDictionary<IntegrationPointDataHubKey, HashSet<string>>();
			}

			if (_updateTimer == null)
			{
				_updateTimer = new Timer(_updateInterval);
				_updateTimer.Elapsed += UpdateTimerElapsed;
				_updateTimer.Start();
			}
		}

		public override Task OnDisconnected(bool stopCalled)
		{
			string removedKeysString = null;
			try
			{
				List<IntegrationPointDataHubKey> removedKeys = RemoveTask(Context.ConnectionId);
				removedKeysString = String.Join(", ", removedKeys);
				_logger.LogVerbose("SignalR task removal completed: {method} (removed keys: {keys})", nameof(OnDisconnected), removedKeysString);
			}
			catch (Exception exception)
			{
				_logger.LogError(exception, "SignalR task removal failed: {method} (removed keys: {keys})", nameof(OnDisconnected), removedKeysString);
			}

			return base.OnDisconnected(stopCalled);
		}

		public void GetIntegrationPointUpdate(int workspaceId, int artifactId)
		{
			int userId = ((ICPHelper)_contextContainer.Helper).GetAuthenticationManager().UserInfo.ArtifactID;
			IntegrationPointDataHubKey key = new IntegrationPointDataHubKey(workspaceId, artifactId, userId);

			AddTask(key);

			_logger.LogVerbose("SignalR add task completed: {method} (key = {key})", nameof(GetIntegrationPointUpdate), key);
		}

		private void UpdateTimerElapsed(object sender, ElapsedEventArgs e)
		{
			try
			{
				_updateTimer.Enabled = false;
				foreach (var key in _tasks.Keys)
				{
					Task.WhenAll(
						UpdateIntegrationPointDataAsync(key),
						UpdateIntegrationPointJobStatusTableAsync(key),
						Task.Delay(_intervalBetweenTasks)       //sleep between getting each stats to get SQL Server a break
					);

				}
			}
			catch (Exception exception)
			{
				_logger.LogError(exception, "SignalR update error in {method}", nameof(UpdateTimerElapsed));
			}
			finally
			{
				_updateTimer.Enabled = true;
			}
		}

		private async Task UpdateIntegrationPointJobStatusTableAsync(IntegrationPointDataHubKey key)
		{
			try
			{
				await Clients.Group(key.ToString()).updateIntegrationPointJobStatusTable();
				_logger.LogVerbose("SignalR update completed: {method} (key = {key})", nameof(UpdateIntegrationPointJobStatusTableAsync), key);
			}
			catch (Exception exception)
			{
				_logger.LogError(exception, "SignalR update error in {method} (key = {key})", nameof(UpdateIntegrationPointJobStatusTableAsync), key);
			}
		}

		private async Task UpdateIntegrationPointDataAsync(IntegrationPointDataHubKey key)
		{
			try
			{
				var permissionRepository =
					new PermissionRepository(ConnectionHelper.Helper(), key.WorkspaceId);
				IRelativityObjectManager objectManager =
					CreateObjectManager(ConnectionHelper.Helper(), key.WorkspaceId);
				var providerTypeService = new ProviderTypeService(objectManager);
				var buttonStateBuilder = new ButtonStateBuilder(providerTypeService, _queueManager,
					_jobHistoryManager, _stateManager, permissionRepository, _permissionValidator,
					objectManager);

				IntegrationPoint integrationPoint = objectManager.Read<IntegrationPoint>(key.IntegrationPointId);

				ProviderType providerType = providerTypeService.GetProviderType(
					integrationPoint.SourceProvider.Value,
					integrationPoint.DestinationProvider.Value);
				bool sourceProviderIsRelativity = providerType == ProviderType.Relativity;

				IntegrationPointModel model = new IntegrationPointModel
				{
					HasErrors = integrationPoint.HasErrors,
					LastRun = integrationPoint.LastRuntimeUTC,
					NextRun = integrationPoint.NextScheduledRuntimeUTC
				};

				IOnClickEventConstructor onClickEventHelper =
					_helperClassFactory.CreateOnClickEventHelper(_managerFactory, _contextContainer);

				ButtonStateDTO buttonStates = buttonStateBuilder.CreateButtonState(key.WorkspaceId, key.IntegrationPointId);
				OnClickEventDTO onClickEvents = onClickEventHelper.GetOnClickEvents(key.WorkspaceId, key.IntegrationPointId,
					integrationPoint.Name, buttonStates);

				await Clients.Group(key.ToString()).updateIntegrationPointData(model, buttonStates, onClickEvents, sourceProviderIsRelativity);

				_logger.LogVerbose("SignalR update completed: {method} (key = {key})", nameof(UpdateIntegrationPointDataAsync), key);
			}
			catch (Exception exception)
			{
				_logger.LogError(exception, "SignalR update error in {method} (key = {key})", nameof(UpdateIntegrationPointDataAsync), key);
			}
		}

		private IRelativityObjectManager CreateObjectManager(ICPHelper helper, int workspaceId)
		{
			return new RelativityObjectManagerFactory(helper).CreateRelativityObjectManager(workspaceId);
		}

		public void AddTask(IntegrationPointDataHubKey key)
		{
			Groups.Add(Context.ConnectionId, key.ToString());
			if (!_tasks.TryAdd(key, new HashSet<string>() { Context.ConnectionId }))
			{
				if (!_tasks[key].Add(Context.ConnectionId))
				{
					_logger.LogDebug("SignalR when adding task: {method} the key is already present", nameof(AddTask));
				}
			}
		}

		private List<IntegrationPointDataHubKey> RemoveTask(string connectionId)
		{
			return _tasks
				.Where(x => x.Value.Contains(connectionId))
				.Select(x => RemoveKey(x.Key))
				.ToList();
		}

		private IntegrationPointDataHubKey RemoveKey(IntegrationPointDataHubKey key)
		{
			Groups.Remove(Context.ConnectionId, key.ToString());
			if (_tasks.ContainsKey(key))
			{
				if (_tasks[key].Contains(Context.ConnectionId))
				{
					_tasks[key].Remove(Context.ConnectionId);
				}
				if (_tasks[key].Count == 0)
				{
					_tasks.TryRemove(key, out _);
				}
			}

			return key;
		}
	}
}