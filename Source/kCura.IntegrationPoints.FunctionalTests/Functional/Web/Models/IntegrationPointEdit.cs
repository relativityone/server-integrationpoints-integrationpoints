namespace Relativity.IntegrationPoints.Tests.Functional.Web.Models
{
	internal class IntegrationPointEdit
	{
		public string Name { get; set; }

		public IntegrationPointTypes Type { get; set; }

		public IntegrationPointDestinations Destination { get; set; }

		public string EmailRecipients { get; set; }
	}
}
