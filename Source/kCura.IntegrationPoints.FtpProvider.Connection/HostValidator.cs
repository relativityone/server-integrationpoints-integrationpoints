using System.Collections.Generic;
using System.Linq;
using System.Net;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.FtpProvider.Connection.Interfaces;

namespace kCura.IntegrationPoints.FtpProvider.Connection
{
    public class HostValidator : IHostValidator
    {
        private readonly IManagerFactory _managerFactory;

        public HostValidator(IManagerFactory managerFactory)
        {
            _managerFactory = managerFactory;
        }

        public bool CanConnectTo(string host)
        {
            IInstanceSettingsManager instanceSettingsManager = _managerFactory.CreateInstanceSettingsManager();
            string blockedIPsSettingValue = instanceSettingsManager.RetrieveBlockedIPs();

            if (!string.IsNullOrWhiteSpace(blockedIPsSettingValue))
            {
                IEnumerable<IPAddress> blockedIPs = GetBlockedIPs(blockedIPsSettingValue);
                List<IPAddress> hostIPs = ResolveIPs(host).ToList();

                bool canConnect = !blockedIPs.Any(blockedIP => hostIPs.Contains(blockedIP));
                return canConnect;
            }
            else
            {
                return true;
            }
        }

        private IEnumerable<IPAddress> GetBlockedIPs(string instanceSettingValue)
        {
            string[] blockedIPs = instanceSettingValue.Split(';');

            foreach (string blockedIP in blockedIPs)
            {
                yield return IPAddress.Parse(blockedIP);
            }
        }

        private IEnumerable<IPAddress> ResolveIPs(string host)
        {
            if (IPAddress.TryParse(host, out IPAddress ip))
            {
                return new []{ip};
            }
            else
            {
                IPHostEntry ipHostEntry = Dns.GetHostEntry(host);
                return ipHostEntry.AddressList;
            }
        }
    }
}