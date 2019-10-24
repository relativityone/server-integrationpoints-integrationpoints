using System.Threading.Tasks;

namespace Relativity.Sync
{
	internal interface IInstanceSettings
	{
		Task<string> GetWebApiPathAsync(string defaultValue = default(string));

		Task<bool> GetRestrictReferentialFileLinksOnImportAsync(bool defaultValue = default(bool));
	}
}