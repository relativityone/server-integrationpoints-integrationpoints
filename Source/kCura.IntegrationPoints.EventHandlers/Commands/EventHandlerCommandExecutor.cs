using System;
using kCura.EventHandler;
using kCura.IntegrationPoints.EventHandlers.Commands.Helpers;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
    public class EventHandlerCommandExecutor
    {
        private readonly IAPILog _logger;

        public EventHandlerCommandExecutor(IAPILog logger)
        {
            _logger = logger;
        }

        public Response Execute(ICommand command)
        {
            Response response = new Response
            {
                Success = true,
                Message = command.SuccessMessage
            };

            try
            {
                command.Execute();
            }
            catch (CommandExecutionException e)
            {
                _logger.LogError(e, "Failed to execute Event Handler Command. {message}", e.Message);
                response.Success = false;
                response.Message = e.Message;
                response.Exception = e;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to execute Event Handler Command. {message}", command.FailureMessage);
                response.Success = false;
                response.Message = command.FailureMessage;
                response.Exception = e;
            }
            return response;
        }
    }
}