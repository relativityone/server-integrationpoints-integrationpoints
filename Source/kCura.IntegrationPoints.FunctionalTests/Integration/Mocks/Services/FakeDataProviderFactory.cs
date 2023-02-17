using Castle.Windsor;
using kCura.IntegrationPoints.Core.Services.Provider;
using Relativity.IntegrationPoints.Contracts.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
    public class FakeDataProviderFactory : IDataProviderFactory
    {
        private readonly WindsorContainer _container;

        public FakeDataProviderFactory(WindsorContainer container)
        {
            _container = container;
        }

        public IDataSourceProvider GetDataProvider(Guid applicationGuid, Guid providerGuid)
        {
            return _container.Resolve<IDataSourceProvider>(providerGuid.ToString());
        }
    }
}
