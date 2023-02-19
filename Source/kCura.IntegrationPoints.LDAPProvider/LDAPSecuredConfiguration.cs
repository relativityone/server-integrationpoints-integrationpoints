using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.LDAPProvider
{
    public class LDAPSecuredConfiguration
    {
        /// <summary>
        /// The user name to use when authenticating the client.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The password to use when authenticating the client.
        /// </summary>
        public string Password { get; set; }
    }
}
