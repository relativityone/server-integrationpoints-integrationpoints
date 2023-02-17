using System;
using Castle.Windsor;
using kCura.IntegrationPoints.EventHandlers.Commands.Container;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
    public class EventHandlerExecutor
    {
        private readonly IContainerFactory _containerFactory;

        public EventHandlerExecutor() : this(new ContainerFactory())
        {
        }

        internal EventHandlerExecutor(IContainerFactory containerFactory)
        {
            _containerFactory = containerFactory;
        }

        public void Execute(IEventHandler eventHandler)
        {
            using (var container = CreateContainer(eventHandler))
            {
                IEHCommand command = ResolveCommand(container, eventHandler);
                command.Execute();
            }
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
            return _containerFactory.Create(eventHandler.Context);
        }
    }
}
