using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Transfer
{
	internal interface IFieldManager
	{
		IList<ISpecialFieldBuilder> SpecialFieldBuilders { get; }
		Task<List<FieldInfo>> GetAllFieldsAsync(CancellationToken token);
		Task<List<FieldInfo>> GetDocumentFieldsAsync(CancellationToken token);
		IEnumerable<FieldInfo> GetSpecialFields();
	}
}