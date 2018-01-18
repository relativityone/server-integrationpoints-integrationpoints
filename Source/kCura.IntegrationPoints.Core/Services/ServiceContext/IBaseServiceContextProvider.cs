using Relativity.Core;

namespace kCura.IntegrationPoints.Core.Services.ServiceContext
{
	public interface IBaseServiceContextProvider
	{
		BaseServiceContext GetUnversionContext(int workspaceArtifactId);
	}
}
