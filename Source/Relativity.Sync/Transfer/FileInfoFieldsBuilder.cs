using System.Collections.Generic;
using System.Linq;
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

			List<INativeFile> nativeFileInfoList = Deduplicate(nativeFileInfo);

			HashSet<int> documentsWithNatives = new HashSet<int>(nativeFileInfoList.Select(x => x.DocumentArtifactId));

			IDictionary<int, INativeFile> artifactIdToNativeFile = nativeFileInfoList.ToDictionary(n => n.DocumentArtifactId);
			
			List<int> documentsWithoutNatives = documentArtifactIds.Where(x => !documentsWithNatives.Contains(x)).ToList();

			artifactIdToNativeFile.AddMany(documentsWithoutNatives, NativeFile.Empty.Repeat());

			return new FileInfoRowValuesBuilder(artifactIdToNativeFile);
		}

		private List<INativeFile> Deduplicate(IEnumerable<INativeFile> natives)
		{
			HashSet<INativeFile> nativesHashSet = new HashSet<INativeFile>(natives);

			HashSet<int> duplicatedArtifactIDs = new HashSet<int>(nativesHashSet
				.GroupBy(x => x.DocumentArtifactId)
				.Where(x => x.Count() > 1)
				.Select(x => x.Key)
			);

			if (duplicatedArtifactIDs.Any())
			{
				_logger.LogWarning("There are {count} documents with duplicated native files in current batch.", duplicatedArtifactIDs.Count);
			}

			nativesHashSet
				.Where(x => duplicatedArtifactIDs.Contains(x.DocumentArtifactId))
				.ForEach(x => x.IsDuplicated = true);

			return nativesHashSet
				.Distinct(new NativeFileByDocumentArtifactIdComparer())
				.ToList();
		}
	}
}