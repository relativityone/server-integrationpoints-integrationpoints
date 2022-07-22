namespace Relativity.Sync.Tests.System.Core
{
    internal class ServiceFactoryFromAppConfig : ServiceFactoryByBasicCredentials
    {
        public ServiceFactoryFromAppConfig() : base(AppSettings.RelativityUserName, AppSettings.RelativityUserPassword)
        {
        }
    }
}