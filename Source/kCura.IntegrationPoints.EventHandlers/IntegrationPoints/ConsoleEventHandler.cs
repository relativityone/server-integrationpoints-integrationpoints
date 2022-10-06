using System;
using kCura.EventHandler;
using kCura.IntegrationPoints.Common.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Core.Managers;
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
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using Relativity.API;
using Console = kCura.EventHandler.Console;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
    public class ConsoleEventHandler : EventHandler.ConsoleEventHandler
    {
        private readonly Guid _agentGuid = new Guid(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID);

        private IButtonStateBuilder _buttonStateBuilder;
        private IManagerFactory _managerFactory;
        private IOnClickEventConstructor _onClickEventConstructor;

        private readonly IConsoleBuilder _consoleBuilder;
        private readonly IHelperClassFactory _helperClassFactory;
        
        public ConsoleEventHandler()
        {
            _helperClassFactory = new HelperClassFactory();
            _consoleBuilder = new ConsoleBuilder();
        }

        internal ConsoleEventHandler(IButtonStateBuilder buttonStateBuilder, IOnClickEventConstructor onClickEventConstructor, IConsoleBuilder consoleBuilder)
        {
            _buttonStateBuilder = buttonStateBuilder;
            _onClickEventConstructor = onClickEventConstructor;
            _consoleBuilder = consoleBuilder;
        }

        public override FieldCollection RequiredFields => new FieldCollection
        {
            new Field(IntegrationPointFields.Name)
        };

        public override void OnButtonClick(ConsoleButton consoleButton)
        {
        }

        public override Console GetConsole(PageEvent pageEvent)
        {
            ButtonStateDTO buttonState = ButtonStateBuilder
                .CreateButtonStateAsync(
                    Application.ArtifactID,
                    ActiveArtifact.ArtifactID)
                .GetAwaiter().GetResult();

            var integrationPointName = ActiveArtifact.Fields[IntegrationPointFields.Name].Value.Value.ToString();

            OnClickEventDTO onClickEvents = OnClickEventConstructor
                .GetOnClickEvents(
                    Application.ArtifactID,
                    ActiveArtifact.ArtifactID,
                    integrationPointName,
                    buttonState);

            return _consoleBuilder.CreateConsole(buttonState, onClickEvents);
        }

        private IManagerFactory ManagerFactory
        {
            get
            {
                if (_managerFactory == null)
                {
                    _managerFactory = new ManagerFactory(Helper, new FakeNonRemovableAgent());
                }

                return _managerFactory;
            }
        }

        private IButtonStateBuilder ButtonStateBuilder
        {
            get
            {
                if (_buttonStateBuilder == null)
                {
                    IAPILog logger = Helper.GetLoggerFactory().GetLogger();
                    IRelativityObjectManager objectManager = CreateObjectManager(Helper, Helper.GetActiveCaseID());
                    IIntegrationPointSerializer integrationPointSerializer = CreateIntegrationPointSerializer(logger);
                    ISecretsRepository secretsRepository = new SecretsRepository(
                        SecretStoreFacadeFactory_Deprecated.Create(Helper.GetSecretStore, logger),
                        logger
                    );
                    IIntegrationPointRepository integrationPointRepository =
                        CreateIntegrationPointRepository(objectManager, integrationPointSerializer, secretsRepository, logger);

                    IQueueManager queueManager = ManagerFactory.CreateQueueManager();
                    IJobHistoryManager jobHistoryManager = ManagerFactory.CreateJobHistoryManager();
                    IStateManager stateManager = ManagerFactory.CreateStateManager();
                    IServicesMgr servicesMgr = Helper.GetServicesManager();
                    IRepositoryFactory repositoryFactory = new RepositoryFactory(Helper, servicesMgr);
                    IIntegrationPointPermissionValidator permissionValidator =
                        new IntegrationPointPermissionValidator(new[]
                            { new ViewErrorsPermissionValidator(repositoryFactory) },
                            new IntegrationPointSerializer(logger));
                    IPermissionRepository permissionRepository = new PermissionRepository(Helper, Helper.GetActiveCaseID());
                    IProviderTypeService providerTypeService = new ProviderTypeService(objectManager);
                    _buttonStateBuilder = new ButtonStateBuilder(providerTypeService, queueManager, jobHistoryManager, stateManager,
                        permissionRepository, permissionValidator, integrationPointRepository, false);
                }
                return _buttonStateBuilder;
            }
        }

        private IRelativityObjectManager CreateObjectManager(IEHHelper helper, int workspaceId)
        {
            return new RelativityObjectManagerFactory(helper).CreateRelativityObjectManager(workspaceId);
        }

        private IIntegrationPointSerializer CreateIntegrationPointSerializer(IAPILog logger)
        {
            return new IntegrationPointSerializer(logger);
        }

        private IIntegrationPointRepository CreateIntegrationPointRepository(
            IRelativityObjectManager objectManager,
            IIntegrationPointSerializer serializer,
            ISecretsRepository secretsRepository,
            IAPILog logger)
        {
            return new IntegrationPointRepository(objectManager, serializer, secretsRepository, logger);
        }

        private IOnClickEventConstructor OnClickEventConstructor
        {
            get
            {
                if (_onClickEventConstructor == null)
                {
                    _onClickEventConstructor = _helperClassFactory.CreateOnClickEventHelper(ManagerFactory);
                }
                return _onClickEventConstructor;
            }
        }
    }
}