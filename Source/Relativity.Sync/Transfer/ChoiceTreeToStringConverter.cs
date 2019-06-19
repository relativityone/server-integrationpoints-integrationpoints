using System.Collections.Generic;
using System.Linq;
using System.Text;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Transfer
{
	internal sealed class ChoiceTreeToStringConverter : IChoiceTreeToStringConverter
	{
		private readonly ISynchronizationConfiguration _config;

		public ChoiceTreeToStringConverter(ISynchronizationConfiguration config)
		{
			_config = config;
		}

		public string ConvertTreeToString(IList<ChoiceWithParentInfo> tree)
		{
			List<StringBuilder> treePaths = new List<StringBuilder>();

			foreach (ChoiceWithParentInfo choice in tree)
			{
				StringBuilder path = new StringBuilder();
				Traverse(choice, treePaths, path);
			}

			string merged = string.Join(char.ToString(_config.ImportSettings.MultiValueDelimiter), treePaths) + _config.ImportSettings.MultiValueDelimiter;
			return merged;
		}

		private void Traverse(ChoiceWithParentInfo choice, List<StringBuilder> paths, StringBuilder path)
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
					StringBuilder newPath = new StringBuilder(path.ToString());
					newPath.Append(_config.ImportSettings.NestedValueDelimiter);
					Traverse(child, paths, newPath);
				}
			}
		}
	}
}