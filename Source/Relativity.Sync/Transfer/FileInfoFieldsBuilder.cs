using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Relativity.Sync.Transfer
{
	internal sealed class FileInfoFieldsBuilder : ISpecialFieldBuilder
	{
		private readonly INativeFileRepository _nativeFileRepository;

		public FileInfoFieldsBuilder(INativeFileRepository nativeFileRepository)
		{
			_nativeFileRepository = nativeFileRepository;
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

			List<INativeFile> nativeFileInfoList = nativeFileInfo.ToList();
			HashSet<int> documentsWithNatives = new HashSet<int>(nativeFileInfoList.Select(x => x.DocumentArtifactId));

			IDictionary<int, INativeFile> artifactIdToNativeFile = nativeFileInfoList.ToDictionary(n => n.DocumentArtifactId);
			List<int> documentsWithoutNatives = documentArtifactIds.Where(x => !documentsWithNatives.Contains(x)).ToList();

			artifactIdToNativeFile.Extend(documentsWithoutNatives, NativeFile.Empty.Repeat());

			return new FileInfoRowValuesBuilder(artifactIdToNativeFile);
		}
	}
}