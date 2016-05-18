namespace kCura.IntegrationPoint.Tests.Core.Models
{
	public class FieldOverlayBehavior
	{
		private FieldOverlayBehavior(string value)
		{
			Value = value;
		}

		public string Value { get; set; }

		public static FieldOverlayBehavior UseFieldSettings => new FieldOverlayBehavior("Use Field Settings");
		public static FieldOverlayBehavior MergeValues => new FieldOverlayBehavior("Merge Values");
		public static FieldOverlayBehavior ReplaceValues => new FieldOverlayBehavior("Replace Values");
	}
}