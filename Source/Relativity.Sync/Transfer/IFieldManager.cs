using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relativity.Sync.Transfer
{
	internal interface IFieldManager
	{
		IList<ISpecialFieldBuilder> SpecialFieldBuilders { get; }
		Task<List<FieldInfo>> GetAllFields();
		Task<List<FieldInfo>> GetDocumentFields();
		IEnumerable<FieldInfo> GetSpecialFields();
	}
}