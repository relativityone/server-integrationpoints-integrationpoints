using System.Collections.Generic;
using System.Linq;
using System.Text;
using kCura.IntegrationPoints.Data.Repositories.DTO;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Sanitization
{
    internal sealed class ChoiceTreeToStringConverter : IChoiceTreeToStringConverter
    {
        private readonly char _multiValueDelimiter;
        private readonly char _nestedValueDelimiter;

        public ChoiceTreeToStringConverter()
        {
            _multiValueDelimiter = IntegrationPoints.Domain.Constants.MULTI_VALUE_DELIMITER;
            _nestedValueDelimiter = IntegrationPoints.Domain.Constants.NESTED_VALUE_DELIMITER;
        }

        public string ConvertTreeToString(IList<ChoiceWithChildInfoDto> choiceTree)
        {
            var treePaths = new List<StringBuilder>();

            foreach (ChoiceWithChildInfoDto choice in choiceTree)
            {
                var path = new StringBuilder();
                Traverse(choice, treePaths, path);
            }

            string merged = string.Join(char.ToString(_multiValueDelimiter), treePaths) + char.ToString(_multiValueDelimiter);
            return merged;
        }

        private void Traverse(ChoiceWithChildInfoDto choice, IList<StringBuilder> paths, StringBuilder path)
        {
            path.Append(choice.Name);

            if (!choice.Children.Any())
            {
                paths.Add(path);
            }
            else
            {
                foreach (ChoiceWithChildInfoDto child in choice.Children)
                {
                    var newPath = new StringBuilder(path.ToString());
                    newPath.Append(_nestedValueDelimiter);
                    Traverse(child, paths, newPath);
                }
            }
        }
    }
}
