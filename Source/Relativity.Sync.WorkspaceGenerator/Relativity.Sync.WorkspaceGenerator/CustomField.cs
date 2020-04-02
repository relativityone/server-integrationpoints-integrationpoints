using Relativity.Services;

namespace Relativity.Sync.WorkspaceGenerator
{
	public class CustomField
	{
		public string Name { get; set; }
		public FieldType Type { get; set; }

		public CustomField(string name, FieldType type)
		{
			Name = name;
			Type = type;
		}
	}
}