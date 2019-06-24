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

		public ChoiceTreeToStringConverter(ISynchronizationConfiguration config)
		{
			_multiValueDelimiter = config.ImportSettings.MultiValueDelimiter;
			_nestedValueDelimiter = config.ImportSettings.NestedValueDelimiter;
		}

		public string ConvertTreeToString(IList<ChoiceWithParentInfo> tree)
		{
			var treePaths = new List<StringBuilder>();

			foreach (ChoiceWithParentInfo choice in tree)
			{
				var path = new StringBuilder();
				Traverse(choice, treePaths, path);
			}

			string merged = string.Join(char.ToString(_multiValueDelimiter), treePaths) + char.ToString(_multiValueDelimiter);
			return merged;
		}

		private void Traverse(ChoiceWithParentInfo choice, IList<StringBuilder> paths, StringBuilder path)
		{
			path.Append(choice.Name);

			if (!choice.Children.Any())
			{
				paths.Add(path);
			}
			else
			{
				foreach (ChoiceWithParentInfo child in choice.Children)
				{
					var newPath = new StringBuilder(path.ToString());
					newPath.Append(_nestedValueDelimiter);
					Traverse(child, paths, newPath);
				}
			}
		}
	}
}