namespace Relativity.Sync.Tests.System.Stubs
{
	internal class ServiceFactoryFromAppConfig : ServiceFactoryByBasicCredentials
	{
		public ServiceFactoryFromAppConfig() : base(AppSettings.RelativityUserName, AppSettings.RelativityUserPassword)
		{
		}
	}
}