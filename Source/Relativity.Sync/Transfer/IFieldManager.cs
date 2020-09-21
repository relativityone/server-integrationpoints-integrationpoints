﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Transfer
{
	internal interface IFieldManager
	{
		Task<FieldInfoDto> GetObjectIdentifierFieldAsync(CancellationToken token);
		
		Task<IReadOnlyList<FieldInfoDto>> GetNativeAllFieldsAsync(CancellationToken token);
		
		Task<IList<FieldInfoDto>> GetDocumentTypeFieldsAsync(CancellationToken token);

		Task<IList<FieldInfoDto>> GetMappedDocumentFieldsAsync(CancellationToken token);
		
		IEnumerable<FieldInfoDto> GetNativeSpecialFields();
		
		IEnumerable<FieldInfoDto> GetImageSpecialFields();

		Task<IDictionary<SpecialFieldType, INativeSpecialFieldRowValuesBuilder>> CreateNativeSpecialFieldRowValueBuildersAsync(int sourceWorkspaceArtifactId, int[] documentArtifactIds);
		
		Task<IDictionary<SpecialFieldType, IImageSpecialFieldRowValuesBuilder>> CreateImageSpecialFieldRowValueBuildersAsync(int sourceWorkspaceArtifactId, int[] documentArtifactIds);
	}
}