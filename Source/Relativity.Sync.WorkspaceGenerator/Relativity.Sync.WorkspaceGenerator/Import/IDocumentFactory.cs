using System.Threading.Tasks;

namespace Relativity.Sync.WorkspaceGenerator.Import
{
	public interface IDocumentFactory
	{
		Task<Document> GetNextDocumentAsync();
	}
}