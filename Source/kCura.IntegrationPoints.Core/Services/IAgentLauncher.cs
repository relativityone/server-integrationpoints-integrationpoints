using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core.Services
{
    public interface IAgentLauncher
    {
        Task LaunchAgentAsync();
    }
}
