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
		private const string _SUPPORTED_BY_VIEWER_FIELD_NAME = "SupportedByViewer";
		private const string _RELATIVITY_NATIVE_TYPE_FIELD_NAME = "RelativityNativeType";
		private readonly INativeFileRepository _nativeFileRepository;

		public FileInfoFieldsBuilder(INativeFileRepository nativeFileRepository)
		{
			_nativeFileRepository = nativeFileRepository;
		}

		public IEnumerable<FieldInfoDto> BuildColumns()
		{
			yield return new FieldInfoDto {SpecialFieldType = SpecialFieldType.NativeFileFilename, DisplayName = _NATIVE_FILE_FILENAME_FIELD_NAME, IsDocumentField = false};
			yield return new FieldInfoDto {SpecialFieldType = SpecialFieldType.NativeFileSize, DisplayName = _NATIVE_FILE_SIZE_FIELD_NAME, IsDocumentField = false};
			yield return new FieldInfoDto {SpecialFieldType = SpecialFieldType.NativeFileLocation, DisplayName = _NATIVE_FILE_LOCATION_FIELD_NAME, IsDocumentField = false};
			yield return new FieldInfoDto {SpecialFieldType = SpecialFieldType.SupportedByViewer, DisplayName = _SUPPORTED_BY_VIEWER_FIELD_NAME, IsDocumentField = true};
			yield return new FieldInfoDto {SpecialFieldType = SpecialFieldType.RelativityNativeType, DisplayName = _RELATIVITY_NATIVE_TYPE_FIELD_NAME, IsDocumentField = true};
		}

		public async Task<ISpecialFieldRowValuesBuilder> GetRowValuesBuilderAsync(int sourceWorkspaceArtifactId, IEnumerable<int> documentArtifactIds)
		{
			List<int> documentArtifactIdsList = documentArtifactIds.ToList();

			IEnumerable<INativeFile> nativeFileInfo = await _nativeFileRepository
				.QueryAsync(sourceWorkspaceArtifactId, documentArtifactIdsList)
				.ConfigureAwait(false);

			List<INativeFile> nativeFileInfoList = nativeFileInfo.ToList();
			HashSet<int> documentsWithNatives = new HashSet<int>(nativeFileInfoList.Select(x => x.DocumentArtifactId));

			IDictionary<int, INativeFile> artifactIdToNativeFile = nativeFileInfoList.ToDictionary(n => n.DocumentArtifactId);
			List<int> documentsWithoutNatives = documentArtifactIdsList.Where(x => !documentsWithNatives.Contains(x)).ToList();

			artifactIdToNativeFile.Extend(documentsWithoutNatives, Enumerable.Repeat(NativeFile.Empty, documentsWithoutNatives.Count));

			return new FileInfoRowValuesBuilder(artifactIdToNativeFile);
		}
	}
}