using kCura.WinEDDS;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Interfaces
{
	public interface IWinEddsBasicLoadFileFactory
	{
		LoadFile GetLoadFile(int workspaceId);
	}
}