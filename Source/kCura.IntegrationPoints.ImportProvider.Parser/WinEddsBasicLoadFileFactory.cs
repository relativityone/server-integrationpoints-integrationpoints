using System.Net;
using kCura.IntegrationPoints.Core.Authentication.WebApi;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using kCura.WinEDDS;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
	public class WinEddsBasicLoadFileFactory : IWinEddsBasicLoadFileFactory
	{
		private readonly IWebApiLoginService _credentialProvider;

		public WinEddsBasicLoadFileFactory(IWebApiLoginService credentialProvider)
		{
			_credentialProvider = credentialProvider;
		}

		public LoadFile GetLoadFile(int workspaceId)
		{
			NetworkCredential cred = _credentialProvider.Authenticate(new CookieContainer());
			//[REL-838809]: Resolve correlationIdFunc
			NativeSettingsFactory factory = new NativeSettingsFactory(cred, workspaceId);
			LoadFile loadFile = factory.ToLoadFile();
			return loadFile;
		}
	}
}