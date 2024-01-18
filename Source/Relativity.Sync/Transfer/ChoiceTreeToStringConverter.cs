using System.Collections.Generic;
using System.Linq;
using System.Text;
using Relativity.Sync.Transfer.ImportAPI;

namespace Relativity.Sync.Transfer
{
    internal sealed class ChoiceTreeToStringConverter : IChoiceTreeToStringConverter
    {
        public string ConvertTreeToString(IList<ChoiceWithChildInfo> tree)
        {
            var treePaths = new List<StringBuilder>();

            foreach (ChoiceWithChildInfo choice in tree)
            {
                var path = new StringBuilder();
                Traverse(choice, treePaths, path);
            }

            string merged = string.Join(char.ToString(LoadFileOptions._DEFAULT_MULTI_VALUE_ASCII), treePaths) + char.ToString(LoadFileOptions._DEFAULT_MULTI_VALUE_ASCII);
            return merged;
        }

        private void Traverse(ChoiceWithChildInfo choice, IList<StringBuilder> paths, StringBuilder path)
        {
            path.Append(choice.Name);

            if (!choice.Children.Any())
            {
                paths.Add(path);
            }
            else
            {
                foreach (ChoiceWithChildInfo child in choice.Children)
                {
                    var newPath = new StringBuilder(path.ToString());
                    newPath.Append(LoadFileOptions._DEFAULT_NESTED_VALUE_ASCII);
                    Traverse(child, paths, newPath);
                }
            }
        }
    }
}
