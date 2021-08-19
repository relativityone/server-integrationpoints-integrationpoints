namespace Relativity.IntegrationPoints.Tests.Functional.Web.Models
{
	internal abstract class IntegrationPointEdit
	{
		public string Name { get; set; }
	}

	internal class IntegrationPointEditImport: IntegrationPointEdit
    {
		public IntegrationPointTypes Type { get; } = IntegrationPointTypes.Import;
		public IntegrationPointSources Source { get; set; }
		public IntegrationPointTransferredObjects TransferredObject { get; set; }
	}

	internal class IntegrationPointEditExport : IntegrationPointEdit
	{
		public IntegrationPointTypes Type { get; } = IntegrationPointTypes.Export;
		public IntegrationPointDestinations Destination { get; set; }
	}
}
