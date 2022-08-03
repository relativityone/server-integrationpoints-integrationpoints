using kCura.IntegrationPoints.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks
{
    public class FakeRelativityUrlHelper: IRelativityUrlHelper
    {
        public string GetRelativityViewUrl(int workspaceID, int artifactID, string objectTypeName)
        {
            return "RelativityViewUrlMock";
        }
    }
}
