using System.Collections.Generic;
using System.Linq;
using System.Text;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Transfer
{
    internal sealed class ChoiceTreeToStringConverter : IChoiceTreeToStringConverter
    {
        private readonly char _multiValueDelimiter;
        private readonly char _nestedValueDelimiter;

        public ChoiceTreeToStringConverter(IDocumentSynchronizationConfiguration config)
        {
            _multiValueDelimiter = config.MultiValueDelimiter;
            _nestedValueDelimiter = config.NestedValueDelimiter;
        }

        public string ConvertTreeToString(IList<ChoiceWithChildInfo> tree)
        {
            var treePaths = new List<StringBuilder>();

            foreach (ChoiceWithChildInfo choice in tree)
            {
                var path = new StringBuilder();
                Traverse(choice, treePaths, path);
            }

            string merged = string.Join(char.ToString(_multiValueDelimiter), treePaths) + char.ToString(_multiValueDelimiter);
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
                    newPath.Append(_nestedValueDelimiter);
                    Traverse(child, paths, newPath);
                }
            }
        }
    }
}
