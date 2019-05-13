using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Vendor.Castle.Components.DictionaryAdapter;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.RestApi;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using IConfiguration = Relativity.Sync.Configuration.IConfiguration;

namespace Relativity.Sync.Transfer
{
	/// <summary>
	/// Creates a single <see cref="DataTable"/> out of several sources of information based on the given schema.
	/// </summary>
	internal sealed class SourceWorkspaceDataTableBuilder
	{
		private readonly MetadataMapping _metadataMapping;
		private readonly IFolderPathRetriever _folderPathRetriever;
		private readonly INativeFileRepository _nativeFileRepository;

		public SourceWorkspaceDataTableBuilder(MetadataMapping mapping, IFolderPathRetriever folderPathRetriever, INativeFileRepository nativeFileRepository)
		{
			_metadataMapping = mapping;
			_folderPathRetriever = folderPathRetriever;
			_nativeFileRepository = nativeFileRepository;
		}

		public async Task<DataTable> BuildAsync(int workspaceArtifactId, RelativityObjectSlim[] batch)
		{
			if (batch == null || !batch.Any())
			{
				return new DataTable();
			}

			DataTable data = new DataTable();
			DataColumn[] columns = _metadataMapping.CreateDataTableColumns();
			data.Columns.AddRange(columns);

			ICollection<int> artifactIdsInBatch = batch.Select(x => x.ArtifactID).ToList();

			// NOTE: This call is not technically needed if we're using DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure,
			// NOTE: but doing it either way greatly simplifies the logic.
			IDictionary<int, string> artifactIdToFolderPath = await _folderPathRetriever
				.GetFolderPathsAsync(workspaceArtifactId, artifactIdsInBatch)
				.ConfigureAwait(false);

			IEnumerable<INativeFile> nativeFileInfo = await _nativeFileRepository
				.QueryAsync(workspaceArtifactId, artifactIdsInBatch)
				.ConfigureAwait(false);
			IDictionary<int, INativeFile> artifactIdToNativeFile = CreateNativeFileMap(artifactIdsInBatch, nativeFileInfo);

			foreach (RelativityObjectSlim obj in batch)
			{
				object[] row = BuildRow(obj, artifactIdToFolderPath, artifactIdToNativeFile);
				data.Rows.Add(row);
			}

			return data;
		}

		private static IDictionary<int, INativeFile> CreateNativeFileMap(IEnumerable<int> artifactIdsInBatch, IEnumerable<INativeFile> nativeFileInfo)
		{
			List<INativeFile> nativeFileInfoList = nativeFileInfo.ToList();
			HashSet<int> documentsWithNatives = new HashSet<int>(nativeFileInfoList.Select(x => x.DocumentArtifactId));

			IDictionary<int, INativeFile> artifactIdToNativeFile = nativeFileInfoList.ToDictionary(n => n.DocumentArtifactId);
			List<int> documentsWithoutNatives = artifactIdsInBatch.Where(x => !documentsWithNatives.Contains(x)).ToList();

			artifactIdToNativeFile.Extend(documentsWithoutNatives, Enumerable.Repeat(NativeFile.Empty, documentsWithoutNatives.Count));

			return artifactIdToNativeFile;
		}

		private object[] BuildRow(RelativityObjectSlim obj, IDictionary<int, string> artifactIdToFolderPath, IDictionary<int, INativeFile> artifactIdToNativeFile)
		{
			string folderPath = artifactIdToFolderPath[obj.ArtifactID];
			INativeFile nativeFileInfo = artifactIdToNativeFile[obj.ArtifactID];
			object[] values = _metadataMapping.CreateDataTableRow(obj.Values, folderPath, nativeFileInfo);
			return values;
		}
	}
}
