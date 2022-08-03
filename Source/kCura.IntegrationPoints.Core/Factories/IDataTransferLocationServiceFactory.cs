using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Services;

namespace kCura.IntegrationPoints.Core.Factories
{
    public interface IDataTransferLocationServiceFactory
    {
        IDataTransferLocationService CreateService(int workspaceId);
    }
}
