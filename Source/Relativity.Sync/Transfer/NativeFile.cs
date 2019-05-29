using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.Services.Interfaces.File;
using Relativity.Services.Interfaces.File.Models;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Transfer
{
	internal sealed class NativeFile : INativeFile
	{
		public NativeFile(int documentArtifactId, string location, string filename, long size)
		{
			DocumentArtifactId = documentArtifactId;
			Location = location;
			Filename = filename;
			Size = size;
		}

		public int DocumentArtifactId { get; }
		public string Location { get; }
		public string Filename { get; }
		public long Size { get; }

		public static INativeFile Empty { get; } = new NativeFile(0, string.Empty, string.Empty, 0);

		public static async Task<IEnumerable<INativeFile>> QueryAsync(ISourceServiceFactoryForUser serviceFactory, int workspaceId, ICollection<int> documentIds)
		{
			if (documentIds == null || !documentIds.Any())
			{
				return Enumerable.Empty<INativeFile>();
			}

			using (IFileManager fileManager = await serviceFactory.CreateProxyAsync<IFileManager>().ConfigureAwait(false))
			{
				FileResponse[] responses = await fileManager.GetNativesForSearchAsync(workspaceId, documentIds.ToArray()).ConfigureAwait(false);
				if (responses == null)
				{
					return Enumerable.Empty<INativeFile>();
				}
				return responses.Select(x => new NativeFile(x.DocumentArtifactID, x.Location, x.Filename, x.Size));
			}
		}
	}
}
