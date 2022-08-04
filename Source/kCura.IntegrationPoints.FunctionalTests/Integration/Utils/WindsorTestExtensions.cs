using Castle.Core;
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Integration.Utils
{
    public static class WindsorTestExtensions
    {
        public static IWindsorContainer ConfigureChangingLifestyleFromPerWebRequestToTransientBecausePerWebRequestIsNotResolvableInTests(
            this IWindsorContainer container)
        {
            container.Kernel.ComponentModelCreated += model =>
            {
                if (model.LifestyleType == LifestyleType.PerWebRequest)
                {
                    model.LifestyleType = LifestyleType.Transient;
                }
            };

            return container;
        }
    }
}
