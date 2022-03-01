using Relativity.API;
using Relativity.DataMigration.MigrateFileshareAccess;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace kCura.ScheduleQueue.Core.Services
{
    public class FileShareSecretProvider : ISecretProvider
    {
		private readonly IHelper _helper;

        public FileShareSecretProvider(IHelper helper)
        {
            _helper = helper;
        }

        public async Task<Dictionary<string, string>> GetAsync(string path)
		{
			ISecretStore secretStore = _helper.GetSecretStore();
			Secret result = await secretStore.GetAsync(path).ConfigureAwait(false);
			return result?.Data;
		}
	}
}
