using System.Collections.Generic;

namespace Relativity.Sync.Transfer
{
    internal interface IChoiceTreeToStringConverter
    {
        string ConvertTreeToString(IList<ChoiceWithChildInfo> tree);
    }
}
