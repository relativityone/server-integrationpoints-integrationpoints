using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.EventHandler;
using kCura.IntegrationPoints.Core;
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
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using kCura.Utility.Extensions;
using Relativity.Toggles;
using Relativity.Toggles.Providers;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	public class ConsoleEventHandler : EventHandler.ConsoleEventHandler
	{
		private readonly IConsoleBuilder _consoleBuilder;
		private readonly IContextContainerFactory _contextContainerFactory;
		private readonly IHelperClassFactory _helperClassFactory;
		private readonly IToggleProvider _toggleProvider;
		private IButtonStateBuilder _buttonStateBuilder;
		private IManagerFactory _managerFactory;
		private IOnClickEventConstructor _onClickEventConstructor;

		public ConsoleEventHandler()
		{
			_contextContainerFactory = new ContextContainerFactory();
			_helperClassFactory = new HelperClassFactory();
			_consoleBuilder = new ConsoleBuilder();
			_toggleProvider = new AlwaysDisabledToggleProvider();
		}

		internal ConsoleEventHandler(IButtonStateBuilder buttonStateBuilder, IOnClickEventConstructor onClickEventConstructor, IConsoleBuilder consoleBuilder)
		{
			_buttonStateBuilder = buttonStateBuilder;
			_onClickEventConstructor = onClickEventConstructor;
			_consoleBuilder = consoleBuilder;
		}

		private IManagerFactory ManagerFactory
		{
			get
			{
				if (_managerFactory == null)
				{
					_managerFactory = new ManagerFactory(Helper, _toggleProvider);
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
					IContextContainer contextContainer = _contextContainerFactory.CreateContextContainer(Helper);
					IQueueManager queueManager = ManagerFactory.CreateQueueManager(contextContainer);
					IJobHistoryManager jobHistoryManager = ManagerFactory.CreateJobHistoryManager(contextContainer);
					IStateManager stateManager = ManagerFactory.CreateStateManager();
					IRepositoryFactory repositoryFactory = new RepositoryFactory(Helper, Helper.GetServicesManager());
					IIntegrationPointPermissionValidator permissionValidator =
						new IntegrationPointPermissionValidator(new[] {new ViewErrorsPermissionValidator(repositoryFactory)}, new JSONSerializer());
					IPermissionRepository permissionRepository = new PermissionRepository(Helper, Helper.GetActiveCaseID());
					IRSAPIService rsapiService = new RSAPIService(Helper, Helper.GetActiveCaseID());
					IProviderTypeService providerTypeService = new ProviderTypeService(rsapiService);
					_buttonStateBuilder = new ButtonStateBuilder(providerTypeService, queueManager, jobHistoryManager, stateManager, permissionRepository, permissionValidator, rsapiService);
				}
				return _buttonStateBuilder;
			}
		}

		private IOnClickEventConstructor OnClickEventConstructor
		{
			get
			{
				if (_onClickEventConstructor == null)
				{
					IContextContainer contextContainer = _contextContainerFactory.CreateContextContainer(Helper);
					_onClickEventConstructor = _helperClassFactory.CreateOnClickEventHelper(ManagerFactory, contextContainer);
				}
				return _onClickEventConstructor;
			}
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
			ButtonStateDTO buttonState = ButtonStateBuilder.CreateButtonState(Application.ArtifactID, ActiveArtifact.ArtifactID);
			var integrationPointName = ActiveArtifact.Fields[IntegrationPointFields.Name].Value.Value.ToString();
			OnClickEventDTO onClickEvents = OnClickEventConstructor.GetOnClickEvents(Application.ArtifactID, ActiveArtifact.ArtifactID,
				integrationPointName, buttonState);

			return _consoleBuilder.CreateConsole(buttonState, onClickEvents);
		}
	}
}