using System.Threading.Tasks;

namespace Relativity.Sync
{
	internal interface IWebApiPathQuery
	{
		Task<string> GetWebApiPathAsync();
	}
}