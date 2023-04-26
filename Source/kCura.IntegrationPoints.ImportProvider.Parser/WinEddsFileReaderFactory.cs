using kCura.WinEDDS;
using kCura.WinEDDS.Api;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using System;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
	public class WinEddsFileReaderFactory : IWinEddsFileReaderFactory
	{
		public IArtifactReader GetLoadFileReader(LoadFile config)
		{
			//[REL-838809]: Resolve correlationIdFunc
			return new LoadFileReader(config, false);
		}

		public IImageReader GetOpticonFileReader(ImageLoadFile config)
		{
			return new OpticonFileReader(0, config, null, Guid.Empty, false);
		}
	}
}
