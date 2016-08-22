namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	using EDDS.DocumentCompareGateway;

	public interface IILongTextStreamFactory
	{
		ILongTextStream CreateLongTextStream(int documentArtifactId, int fieldArtifactId);
	}
}