using Relativity;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class ViewFieldInfoFieldTypeExtender
	{
		public ViewFieldInfo Value { get; }

		public ViewFieldInfoFieldTypeExtender(ViewFieldInfo viewFieldInfo)
		{
			Value = viewFieldInfo;
			FieldTypeAsString = viewFieldInfo.FieldType.ToString();
		}

		public string FieldTypeAsString { get; }
	}
}