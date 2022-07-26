using System;
using System.Text;

namespace Relativity.Sync.Dashboards
{
    public class BasicAuthTokenGenerator : IAuthTokenGenerator
    {
        public string GetAuthToken(string userName, string password)
        {
            string token = Convert.ToBase64String(Encoding.UTF8.GetBytes(userName + ":" + password));
            return $"Basic {token}";
        }
    }
}