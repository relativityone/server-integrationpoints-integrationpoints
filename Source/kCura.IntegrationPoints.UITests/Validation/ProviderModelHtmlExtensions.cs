namespace kCura.IntegrationPoints.UITests.Validation
{
	using System;

	public static class ProviderModelHtmlExtensions
	{
		public static string AsHtmlString(this bool @this)
		{
			return @this ? "Yes" : "No";
		}

		public static string AsHtmlString(this object @this)
		{
			return @this == null ? string.Empty : @this.ToString();
		}
	}
}