using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Models
{
    internal class ImportFromLDAPConnectToSource
    {
        public string ConnectionPath { get; set; }

        public IntegrationPointAuthentication Authentication{get; private set;}

        public string Username { get; private set; }

        public string Password { get; private set; }
    }
}
