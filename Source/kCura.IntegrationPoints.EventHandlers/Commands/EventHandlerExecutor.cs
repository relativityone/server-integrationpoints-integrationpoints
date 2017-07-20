using System;
using Castle.Windsor;
using kCura.EventHandler;
using kCura.IntegrationPoints.EventHandlers.Commands.Container;
using kCura.IntegrationPoints.EventHandlers.Commands.Helpers;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
	public class EventHandlerExecutor
	{
		private readonly IContainerFactory _containerFactory;
		private const string _COMMAND_FAILURE_MESSAGE = "Event Handler Command failed.";
		private const string _UNKNOWN_FAILURE_MESSAGE = "Unknown exception during Event Handler Command execution.";

		public EventHandlerExecutor() : this(new ContainerFactory())
		{
		}

		internal EventHandlerExecutor(IContainerFactory containerFactory)
		{
			_containerFactory = containerFactory;
		}

		public Response Execute(IEventHandler eventHandler)
		{
			var response = new Response
			{
				Message = eventHandler.SuccessMessage,
				Success = true
			};
			try
			{
				using (var container = CreateContainer(eventHandler))
				{
					IEHCommand command = ResolveCommand(container, eventHandler);
					command.Execute();
				}
			}
			catch (CommandExecutionException e)
			{
				LogException(eventHandler, e, _COMMAND_FAILURE_MESSAGE);
				response.Message = e.Message;
				response.Exception = e;
				response.Success = false;
			}
			catch (Exception e)
			{
				LogException(eventHandler, e, _UNKNOWN_FAILURE_MESSAGE);
				response.Message = eventHandler.FailureMessage;
				response.Exception = e;
				response.Success = false;
			}
			return response;
		}

		private IEHCommand ResolveCommand(IWindsorContainer container, IEventHandler eventHandler)
		{
			IEHCommand command = container.Resolve(eventHandler.CommandType) as IEHCommand;
			if (command == null)
			{
				throw new ArgumentException("Cannot resolve specified command.");
			}
			return command;
		}

		private IWindsorContainer CreateContainer(IEventHandler eventHandler)
		{
			return _containerFactory.Create(eventHandler.Helper);
		}

		private void LogException(IEventHandler eventHandler, Exception e, string message)
		{
			eventHandler.Helper.GetLoggerFactory().GetLogger().ForContext(eventHandler.CommandType).LogError(e, message);
		}
	}
}