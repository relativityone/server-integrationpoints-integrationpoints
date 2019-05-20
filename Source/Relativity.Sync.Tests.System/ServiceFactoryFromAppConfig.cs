namespace Relativity.Sync.Tests.System
{
	internal class ServiceFactoryFromAppConfig : ServiceFactoryByBasicCredentials
	{
		public ServiceFactoryFromAppConfig() : base(AppSettings.RelativityUserName, AppSettings.RelativityUserPassword)
		{
		}
	}
}