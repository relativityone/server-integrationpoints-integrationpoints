using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Transfer
{
	/// <summary>
	/// Creates a single <see cref="DataTable"/> out of several sources of information based on the given schema.
	/// </summary>
	internal sealed class SourceWorkspaceDataTableBuilder : ISourceWorkspaceDataTableBuilder
	{
		private readonly MetadataMapping _metadataMapping;
		private readonly int _sourceJobArtifactId;
		private readonly int _sourceWorkspaceArtifactId;
		private readonly DestinationFolderStructureBehavior _destinationFolderStructureBehavior;
		private readonly IFolderPathRetriever _folderPathRetriever;
		private readonly INativeFileRepository _nativeFileRepository;

		public SourceWorkspaceDataTableBuilder(SourceDataReaderConfiguration configuration, IFolderPathRetriever folderPathRetriever, INativeFileRepository nativeFileRepository)
		{
			_metadataMapping = configuration.MetadataMapping;
			_sourceJobArtifactId = configuration.SourceJobId;
			_sourceWorkspaceArtifactId = configuration.SourceWorkspaceId;
			_destinationFolderStructureBehavior = configuration.DestinationFolderStructureBehavior;
			_folderPathRetriever = folderPathRetriever;
			_nativeFileRepository = nativeFileRepository;
		}

		public async Task<DataTable> BuildAsync(RelativityObjectSlim[] batch)
		{
			if (batch == null || !batch.Any())
			{
				return new DataTable();
			}

			DataTable data = new DataTable();

			DataColumn[] columns = BuildColumns();
			data.Columns.AddRange(columns);

			IDictionary<int, string> artifactIdToFolderPath = await CreateFolderPathMapAsync(batch).ConfigureAwait(false);
			IDictionary<int, INativeFile> artifactIdToNativeFile = await CreateNativeFileMapAsync(batch).ConfigureAwait(false);

			foreach (RelativityObjectSlim obj in batch)
			{
				string folderPath = artifactIdToFolderPath[obj.ArtifactID];
				INativeFile nativeFileInfo = artifactIdToNativeFile[obj.ArtifactID];
				object[] row = BuildRow(obj, folderPath, nativeFileInfo);
				data.Rows.Add(row);
			}

			return data;
		}

		private DataColumn[] BuildColumns()
		{
			IEnumerable<FieldEntry> documentFields = _metadataMapping.GetDestinationDocumentFields();
			IEnumerable<FieldEntry> specialFields = _metadataMapping.GetSpecialFields();

			DataColumn[] columns = documentFields.Concat(specialFields)
				.Select(x => new DataColumn(x.DisplayName))
				.ToArray();
			return columns;
		}

		private async Task<IDictionary<int, string>> CreateFolderPathMapAsync(RelativityObjectSlim[] batch)
		{
			List<int> artifactIds = batch.Select(x => x.ArtifactID).ToList();
			IDictionary<int, string> folderPathsMap;
			switch (_destinationFolderStructureBehavior)
			{
				case DestinationFolderStructureBehavior.ReadFromField:
					List<FieldEntry> documentFields = _metadataMapping.GetSourceDocumentFields().ToList();
					int folderPathSourceFieldIndex = documentFields.FindIndex(f => f.SpecialFieldType == SpecialFieldType.ReadFromFieldFolderPath);
					IEnumerable<string> folderPaths = batch.Select(x => (string) x.Values[folderPathSourceFieldIndex]);
					folderPathsMap = artifactIds.MapOnto(folderPaths);
					break;
				case DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure:
					folderPathsMap = await _folderPathRetriever
						.GetFolderPathsAsync(_sourceWorkspaceArtifactId, artifactIds)
						.ConfigureAwait(false);
					break;
				default:
					folderPathsMap = artifactIds.MapOnto(Enumerable.Repeat(string.Empty, artifactIds.Count));
					break;
			}

			return folderPathsMap;
		}

		private async Task<IDictionary<int, INativeFile>> CreateNativeFileMapAsync(RelativityObjectSlim[] batch)
		{
			List<int> batchArtifactIds = batch.Select(x => x.ArtifactID).ToList();

			IEnumerable<INativeFile> nativeFileInfo = await _nativeFileRepository
				.QueryAsync(_sourceWorkspaceArtifactId, batchArtifactIds)
				.ConfigureAwait(false);

			List<INativeFile> nativeFileInfoList = nativeFileInfo.ToList();
			HashSet<int> documentsWithNatives = new HashSet<int>(nativeFileInfoList.Select(x => x.DocumentArtifactId));

			IDictionary<int, INativeFile> artifactIdToNativeFile = nativeFileInfoList.ToDictionary(n => n.DocumentArtifactId);
			List<int> documentsWithoutNatives = batchArtifactIds.Where(x => !documentsWithNatives.Contains(x)).ToList();

			artifactIdToNativeFile.Extend(documentsWithoutNatives, Enumerable.Repeat(NativeFile.Empty, documentsWithoutNatives.Count));

			return artifactIdToNativeFile;
		}

		private object[] BuildRow(RelativityObjectSlim obj, string folderPath, INativeFile nativeFileInfo)
		{
			List<object> documentFieldValues = obj.Values.ToList();

			IEnumerable<FieldEntry> specialFields = _metadataMapping.GetSpecialFields();
			IEnumerable<object> specialFieldValues = specialFields.Select<FieldEntry, object>(x =>
			{
				switch (x.SpecialFieldType)
				{

					case SpecialFieldType.FolderPath:
						return folderPath;
					case SpecialFieldType.NativeFileSize:
						return nativeFileInfo.Size;
					case SpecialFieldType.NativeFileLocation:
						return nativeFileInfo.Location;
					case SpecialFieldType.NativeFileFilename:
						return nativeFileInfo.Filename;
					case SpecialFieldType.SourceJob:
						return _sourceJobArtifactId;
					case SpecialFieldType.SourceWorkspace:
						return _sourceWorkspaceArtifactId;
					default:
						// TODO: Should throw here; we should fail ASAP if we encounter a field we can't map
						return null;
				}
			});

			IEnumerable<object> values = documentFieldValues.Concat(specialFieldValues);
			return values.ToArray();
		}
	}
}
