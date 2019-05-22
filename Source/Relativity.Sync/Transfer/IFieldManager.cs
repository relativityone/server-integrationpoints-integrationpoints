using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Transfer
{
	internal interface IFieldManager
	{
		Task<FieldInfoDto> GetObjectIdentifierFieldAsync(CancellationToken token);
		Task<IList<FieldInfoDto>> GetAllFieldsAsync(CancellationToken token);
		Task<IList<FieldInfoDto>> GetDocumentFieldsAsync(CancellationToken token);
		IEnumerable<FieldInfoDto> GetSpecialFields();
		Task<IDictionary<SpecialFieldType, ISpecialFieldRowValuesBuilder>> CreateSpecialFieldRowValueBuildersAsync(int sourceWorkspaceArtifactId, ICollection<int> documentArtifactIds);
	}
}