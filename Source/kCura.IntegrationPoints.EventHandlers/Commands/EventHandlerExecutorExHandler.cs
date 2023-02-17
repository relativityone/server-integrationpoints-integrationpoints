using System;
using Castle.Windsor;
using kCura.EventHandler;
using kCura.IntegrationPoints.EventHandlers.Commands.Container;
using kCura.IntegrationPoints.EventHandlers.Commands.Helpers;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
    public class EventHandlerExecutorExHandler : EventHandlerExecutor
    {
        private const string _COMMAND_FAILURE_MESSAGE = "Event Handler Command failed.";
        private const string _UNKNOWN_FAILURE_MESSAGE = "Unknown exception during Event Handler Command execution.";

        public EventHandlerExecutorExHandler() : this(new ContainerFactory())
        {
        }

        internal EventHandlerExecutorExHandler(IContainerFactory containerFactory): base(containerFactory)
        {
        }

        public Response Execute(IEventHandlerEx eventHandler)
        {
            var response = new Response
            {
                Message = eventHandler.SuccessMessage,
                Success = true
            };
            try
            {
                base.Execute(eventHandler);
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

        private void LogException(IEventHandler eventHandler, Exception e, string message)
        {
            eventHandler.Context.Helper.GetLoggerFactory().GetLogger().ForContext(eventHandler.CommandType).LogError(e, message);
        }
    }
}
