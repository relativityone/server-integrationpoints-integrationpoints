using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoint.Tests.Core.Validators
{
	public interface IFolderPathStrategy
	{
		string GetFolderPath(Document document);
	}
}