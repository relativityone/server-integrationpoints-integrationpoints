using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using kCura.IntegrationPoints.Common.Agent;
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
using kCura.IntegrationPoints.Data.Facades.SecretStore.Implementation;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Helpers;
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

        private readonly ICPHelper _helper;
        private readonly IAPILog _logger;
        private readonly IHelperClassFactory _helperClassFactory;
        private readonly IIntegrationPointPermissionValidator _permissionValidator;
        private readonly IJobHistoryManager _jobHistoryManager;
        private readonly IManagerFactory _managerFactory;
        private readonly int _intervalBetweenTasks = 100;
        private readonly int _updateInterval = 5000;
        private readonly IQueueManager _queueManager;
        private readonly IStateManager _stateManager;
        private readonly IServicesMgr _serviceManager;

        public IntegrationPointDataHub() : this(ConnectionHelper.Helper(), new HelperClassFactory())
        { }

        internal IntegrationPointDataHub(ICPHelper helper, IHelperClassFactory helperClassFactory)
        {
            _helper = helper;
            _helperClassFactory = helperClassFactory;

            _logger = _helper.GetLoggerFactory().GetLogger();

            _managerFactory = new ManagerFactory(_helper, new FakeNonRemovableAgent());
            _queueManager = _managerFactory.CreateQueueManager();
            _jobHistoryManager = _managerFactory.CreateJobHistoryManager();
            _stateManager = _managerFactory.CreateStateManager();
            _serviceManager = _helper.GetServicesManager();

            IRepositoryFactory repositoryFactory = new RepositoryFactory(_helper, _serviceManager);
            _permissionValidator = new IntegrationPointPermissionValidator(new[]{new ViewErrorsPermissionValidator(repositoryFactory)}, new IntegrationPointSerializer(_logger));

            if (_tasks == null)
            {
                _tasks = new ConcurrentDictionary<IntegrationPointDataHubKey, HashSet<string>>();
            }

            IRelativityObjectManager objectManager = new RelativityObjectManagerFactory(helper).CreateRelativityObjectManager(helper.GetActiveCaseID());
            LiquidFormsHelper liquidFormsHelper = new LiquidFormsHelper(_serviceManager, _logger, objectManager);
            bool isLiquidFormsEnabled = liquidFormsHelper.IsLiquidForms(helper.GetActiveCaseID()).GetAwaiter().GetResult();

            if (_updateTimer == null && !isLiquidFormsEnabled)
            {
                _updateTimer = new Timer(_updateInterval);
                _updateTimer.Elapsed += UpdateTimerElapsed;
                _updateTimer.Start();
            }

            if (_updateTimer != null && isLiquidFormsEnabled)
            {
                _updateTimer.Stop();
                _updateTimer.Close();
                _updateTimer = null;
            }
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            string removedKeysString = null;
            try
            {
                List<IntegrationPointDataHubKey> removedKeys = RemoveTask(Context.ConnectionId);
                removedKeysString = String.Join(", ", removedKeys);
                _logger.LogInformation("SignalR task removal completed: {method} (removed keys: {keys})", nameof(OnDisconnected), removedKeysString);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "SignalR task removal failed: {method} (removed keys: {keys})", nameof(OnDisconnected), removedKeysString);
            }

            return base.OnDisconnected(stopCalled);
        }

        public void GetIntegrationPointUpdate(int workspaceId, int artifactId)
        {
            int userId = _helper.GetAuthenticationManager().UserInfo.ArtifactID;
            IntegrationPointDataHubKey key = new IntegrationPointDataHubKey(workspaceId, artifactId, userId);

            AddTask(key);

            _logger.LogInformation("SignalR add task completed: {method} (key = {key})", nameof(GetIntegrationPointUpdate), key);
        }

        public void AddTask(IntegrationPointDataHubKey key)
        {
            Groups.Add(Context.ConnectionId, key.ToString());
            if (!_tasks.TryAdd(key, new HashSet<string>() { Context.ConnectionId }))
            {
                if (!_tasks[key].Add(Context.ConnectionId))
                {
                    _logger.LogInformation("SignalR when adding task: {method} the key is already present", nameof(AddTask));
                }
            }
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
                        Task.Delay(_intervalBetweenTasks) //sleep between getting each stats to get SQL Server a break
                    ).GetAwaiter().GetResult();
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
                ICPHelper helper = ConnectionHelper.Helper();
                var permissionRepository = new PermissionRepository(helper, key.WorkspaceId);
                IRelativityObjectManager objectManager = CreateObjectManager(helper, key.WorkspaceId);
                IAPILog logger = ConnectionHelper.Helper().GetLoggerFactory().GetLogger();
                IIntegrationPointSerializer integrationPointSerializer = CreateIntegrationPointSerializer(logger);
                ISecretsRepository secretsRepository = new SecretsRepository(
                    SecretStoreFacadeFactory_Deprecated.Create(helper.GetSecretStore, logger),
                    logger
                );
                IIntegrationPointRepository integrationPointRepository =
                    CreateIntegrationPointRepository(objectManager, integrationPointSerializer, secretsRepository, logger);
                var providerTypeService = new ProviderTypeService(objectManager);
                var buttonStateBuilder = new ButtonStateBuilder(providerTypeService, _queueManager, _jobHistoryManager,
                    _stateManager, permissionRepository, _permissionValidator, integrationPointRepository);

                IntegrationPoint integrationPoint = await integrationPointRepository
                    .ReadEncryptedAsync(key.IntegrationPointId)
                    .ConfigureAwait(false);

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
                    _helperClassFactory.CreateOnClickEventHelper(_managerFactory);

                ButtonStateDTO buttonStates = buttonStateBuilder.CreateButtonState(key.WorkspaceId, key.IntegrationPointId);
                OnClickEventDTO onClickEvents = onClickEventHelper.GetOnClickEvents(key.WorkspaceId, key.IntegrationPointId,
                    integrationPoint.Name, buttonStates);

                await Clients.Group(key.ToString()).updateIntegrationPointData(model, buttonStates, onClickEvents, sourceProviderIsRelativity);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "SignalR update error in {method} (key = {key})", nameof(UpdateIntegrationPointDataAsync), key);
            }
        }

        private static IRelativityObjectManager CreateObjectManager(ICPHelper helper, int workspaceId)
        {
            return new RelativityObjectManagerFactory(helper).CreateRelativityObjectManager(workspaceId);
        }

        private static IIntegrationPointSerializer CreateIntegrationPointSerializer(IAPILog logger)
        {
            return new IntegrationPointSerializer(logger);
        }

        private static IIntegrationPointRepository CreateIntegrationPointRepository(
            IRelativityObjectManager relativityObjectManager,
            IIntegrationPointSerializer serializer,
            ISecretsRepository secretsRepository,
            IAPILog logger)
        {
            return new IntegrationPointRepository(
                relativityObjectManager,
                serializer,
                secretsRepository,
                logger);
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
                if (_tasks[key].Remove(Context.ConnectionId))
                {
                    if (_tasks[key].Count == 0)
                    {
                        HashSet<string> result; // it's not `out _` because build.ps1 uses some old MSBuild, that does not support new and fancy language features
                        _tasks.TryRemove(key, out result);
                    }
                }
            }

            return key;
        }
    }
}