using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.RestApi;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Transfer
{
	/// <summary>
	/// Creates a single <see cref="DataTable"/> out of several sources of information based on the given schema.
	/// </summary>
	internal sealed class SourceWorkspaceDataTableBuilder
	{
		private readonly MetadataMapping _metadataMapping;
		private readonly IFolderPathRetriever _folderPathRetriever;
		
		public SourceWorkspaceDataTableBuilder(MetadataMapping mapping, IFolderPathRetriever folderPathRetriever)
		{
			_metadataMapping = mapping;
			_folderPathRetriever = folderPathRetriever;
		}

		public async Task<DataTable> BuildAsync(int workspaceArtifactId, RelativityObjectSlim[] batch)
		{
			DataTable data = new DataTable();

			DataColumn[] columns = _metadataMapping.GetColumnsForDataTable();
			data.Columns.AddRange(columns);

			IDictionary<int, string> artifactIdToFolderPath;

			if (_metadataMapping.DestinationFolderStructureBehavior == DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure)
			{
				artifactIdToFolderPath = await _folderPathRetriever.GetFolderPathsAsync(workspaceArtifactId, batch.Select(x => x.ArtifactID).ToList()).ConfigureAwait(false);
			}
			else
			{
				artifactIdToFolderPath = batch.ToDictionary(x => x.ArtifactID, _ => _metadataMapping.FolderPathFieldName);
			}

			foreach (RelativityObjectSlim obj in batch)
			{
				object[] row = BuildRow(obj, artifactIdToFolderPath);
				data.Rows.Add(row);
			}

			return data;
		}

		private DataColumn[] BuildSpecialDataColumns()
		{
			return new[]
			{
				new DataColumn("SourceWorkspaceFolderPath"), // 
				new DataColumn("NativeFilePath"), // NativeFileImportService.SourceFieldName
				new DataColumn("FileName"), // ImportSettings.FileNameColumn
				new DataColumn("NativeFileSize") // ImportSettings.FileSizeColumn
			};
		}

		private object[] BuildRow(RelativityObjectSlim obj, IDictionary<int, string> artifactIdToFolderPath)
		{
			IEnumerable<object> values = CreateValues(obj.Values, artifactIdToFolderPath[obj.ArtifactID]);
			return values.ToArray();
		}

		private IEnumerable<object> CreateValues(List<object> fieldValues, string folderPath)
		{
			foreach (object val in fieldValues)
			{
				yield return val;
			}

			yield return folderPath;
		}
	}
}
