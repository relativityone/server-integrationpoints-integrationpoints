using System.Threading.Tasks;
using Relativity.Services.ObjectQuery;

namespace kCura.IntegrationPoints.Core.Services.RDO
{
	public interface IRDORepository
	{
		Task<ObjectQueryResutSet> RetrieveAsync(Query query, string queryToken, int startIndex = 1, int pageSize = 1000);
	}
}