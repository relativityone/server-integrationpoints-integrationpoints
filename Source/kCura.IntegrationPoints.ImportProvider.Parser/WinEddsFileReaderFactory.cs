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
			return new LoadFileReader(config, false, () => string.Empty);
		}

		public IImageReader GetOpticonFileReader(ImageLoadFile config)
		{
			return new OpticonFileReader(0, config, null, Guid.Empty, false);
		}
	}
}
