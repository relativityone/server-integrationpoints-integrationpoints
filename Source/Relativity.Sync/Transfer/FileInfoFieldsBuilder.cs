using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relativity.Sync.Transfer
{
	internal sealed class FileInfoFieldsBuilder : ISpecialFieldBuilder
	{
		private readonly INativeFileRepository _nativeFileRepository;
		private readonly ISyncLog _logger;

		public FileInfoFieldsBuilder(INativeFileRepository nativeFileRepository, ISyncLog logger)
		{
			_nativeFileRepository = nativeFileRepository;
			_logger = logger;
		}

		public IEnumerable<FieldInfoDto> BuildColumns()
		{
			yield return FieldInfoDto.NativeFileFilenameField();
			yield return FieldInfoDto.NativeFileSizeField();
			yield return FieldInfoDto.NativeFileLocationField();
			yield return FieldInfoDto.SupportedByViewerField();
			yield return FieldInfoDto.RelativityNativeTypeField();
		}

		public async Task<ISpecialFieldRowValuesBuilder> GetRowValuesBuilderAsync(int sourceWorkspaceArtifactId, ICollection<int> documentArtifactIds)
		{
			IEnumerable<INativeFile> nativeFileInfo = await _nativeFileRepository
				.QueryAsync(sourceWorkspaceArtifactId, documentArtifactIds)
				.ConfigureAwait(false);

			IDictionary<int, INativeFile> artifactIdToNativeFile = new Dictionary<int, INativeFile>(documentArtifactIds.Count);
			int documentsWithDuplicatedNativesCount = 0;

			foreach (INativeFile nativeFile in nativeFileInfo)
			{
				if (artifactIdToNativeFile.ContainsKey(nativeFile.DocumentArtifactId))
				{
					documentsWithDuplicatedNativesCount++;
					artifactIdToNativeFile[nativeFile.DocumentArtifactId].IsDuplicated = true;
				}
				else
				{
					artifactIdToNativeFile.Add(nativeFile.DocumentArtifactId, nativeFile);
				}
			}
			
			_logger.LogWarning("There are {count} documents with duplicated native files in current batch.", documentsWithDuplicatedNativesCount);

			return new FileInfoRowValuesBuilder(artifactIdToNativeFile);
		}
	}
}