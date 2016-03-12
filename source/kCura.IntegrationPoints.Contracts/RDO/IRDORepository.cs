using System.Threading.Tasks;
using Relativity.Services.ObjectQuery;

namespace kCura.IntegrationPoints.Contracts.RDO
{
	public interface IRDORepository
	{
		Task<ObjectQueryResultSet> RetrieveAsync(Query query, string queryToken, int startIndex = 1, int pageSize = 1000);
	}
}