using System.Threading.Tasks;
using Refit;

namespace Relativity.Sync.Dashboards.Api
{
	[Headers("Accept: application/json")]
	public interface IJiraApi
	{
		[Get("/rest/api/2/issue/{issueIdOrKey}")]
		Task<object> GetIssueAsync(string issueIdOrKey);
	}
}