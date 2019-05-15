using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
	internal sealed class FileInfoFieldsBuilder : ISpecialFieldBuilder
	{
		private const string _NATIVE_FILE_LOCATION_FIELD_NAME = "NativeFileLocation";
		private const string _NATIVE_FILE_SIZE_FIELD_NAME = "NativeFileSize";
		private const string _NATIVE_FILE_FILENAME_FIELD_NAME = "NativeFileFilename";
		private readonly INativeFileRepository _nativeFileRepository;

		public FileInfoFieldsBuilder(INativeFileRepository nativeFileRepository)
		{
			_nativeFileRepository = nativeFileRepository;
		}

		public IEnumerable<FieldInfo> BuildColumns()
		{
			yield return new FieldInfo {SpecialFieldType = SpecialFieldType.NativeFileFilename, DisplayName = _NATIVE_FILE_FILENAME_FIELD_NAME, IsDocumentField = false};
			yield return new FieldInfo {SpecialFieldType = SpecialFieldType.NativeFileSize, DisplayName = _NATIVE_FILE_SIZE_FIELD_NAME, IsDocumentField = false};
			yield return new FieldInfo {SpecialFieldType = SpecialFieldType.NativeFileLocation, DisplayName = _NATIVE_FILE_LOCATION_FIELD_NAME, IsDocumentField = false};
		}

		public async Task<ISpecialFieldRowValuesBuilder> GetRowValuesBuilderAsync(int sourceWorkspaceArtifactId, RelativityObjectSlim[] documents)
		{
			List<int> batchArtifactIds = documents.Select(x => x.ArtifactID).ToList();

			IEnumerable<INativeFile> nativeFileInfo = await _nativeFileRepository
				.QueryAsync(sourceWorkspaceArtifactId, batchArtifactIds)
				.ConfigureAwait(false);

			List<INativeFile> nativeFileInfoList = nativeFileInfo.ToList();
			HashSet<int> documentsWithNatives = new HashSet<int>(nativeFileInfoList.Select(x => x.DocumentArtifactId));

			IDictionary<int, INativeFile> artifactIdToNativeFile = nativeFileInfoList.ToDictionary(n => n.DocumentArtifactId);
			List<int> documentsWithoutNatives = batchArtifactIds.Where(x => !documentsWithNatives.Contains(x)).ToList();

			artifactIdToNativeFile.Extend(documentsWithoutNatives, Enumerable.Repeat(NativeFile.Empty, documentsWithoutNatives.Count));

			return new FileInfoRowValuesBuilder(artifactIdToNativeFile);
		}
	}
}