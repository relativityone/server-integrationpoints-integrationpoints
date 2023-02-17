using System;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using Castle.Windsor;
using Relativity.API;

namespace kCura.IntegrationPoints.Web
{
    public class WindsorCompositionRoot : IHttpControllerActivator
    {
        private readonly IWindsorContainer _container;
        private readonly IAPILog _logger;

        public WindsorCompositionRoot(IWindsorContainer container, IAPILog logger)
        {
            _container = container;
            _logger = logger.ForContext<WindsorCompositionRoot>();
        }

        public IHttpController Create(
                HttpRequestMessage request,
                HttpControllerDescriptor controllerDescriptor,
                Type controllerType)
        {
            try
            {
                IHttpController controller = (IHttpController)_container.Resolve(controllerType);

                request.RegisterForDispose(new Release(() => _container.Release(controller)));

                return controller;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resolve {component}", nameof(IHttpController));
                throw;
            }
        }

        private class Release : IDisposable
        {
            private readonly Action release;

            public Release(Action release)
            {
                this.release = release;
            }

            public void Dispose()
            {
                release();
            }
        }
    }
}
