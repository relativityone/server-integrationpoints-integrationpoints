using kCura.EventHandler;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	public class ConsoleEventHandler : EventHandler.ConsoleEventHandler
	{
		private readonly IConsoleBuilder _consoleBuilder;
		private readonly IContextContainerFactory _contextContainerFactory;
		private readonly IHelperClassFactory _helperClassFactory;
		private IButtonStateBuilder _buttonStateBuilder;
		private IManagerFactory _managerFactory;
		private IOnClickEventConstructor _onClickEventConstructor;

		public ConsoleEventHandler()
		{
			_contextContainerFactory = new ContextContainerFactory();
			_helperClassFactory = new HelperClassFactory();
			_consoleBuilder = new ConsoleBuilder();
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
					_managerFactory = new ManagerFactory(Helper);
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
					IIntegrationPointManager integrationPointManager = ManagerFactory.CreateIntegrationPointManager(contextContainer);
					IQueueManager queueManager = ManagerFactory.CreateQueueManager(contextContainer);
					IJobHistoryManager jobHistoryManager = ManagerFactory.CreateJobHistoryManager(contextContainer);
					IStateManager stateManager = ManagerFactory.CreateStateManager();
					IPermissionRepository permissionRepository = new PermissionRepository(Helper, Helper.GetActiveCaseID());
					_buttonStateBuilder = new ButtonStateBuilder(integrationPointManager, queueManager, jobHistoryManager, stateManager, permissionRepository);
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

		public override FieldCollection RequiredFields => new FieldCollection();

		public override void OnButtonClick(ConsoleButton consoleButton)
		{
		}

		public override Console GetConsole(PageEvent pageEvent)
		{
			ButtonStateDTO buttonState = ButtonStateBuilder.CreateButtonState(Application.ArtifactID, ActiveArtifact.ArtifactID);
			OnClickEventDTO onClickEvents = OnClickEventConstructor.GetOnClickEvents(Application.ArtifactID, ActiveArtifact.ArtifactID, buttonState);

			return _consoleBuilder.CreateConsole(buttonState, onClickEvents);
		}
	}
}