namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
	public class ExportProcessRunner
	{
		private readonly IExportProcessBuilder _exportProcessBuilder;

		public ExportProcessRunner(IExportProcessBuilder exportProcessBuilder)
		{
			_exportProcessBuilder = exportProcessBuilder;
		}

		public void StartWith(ExportSettings settings)
		{
			var exporter = _exportProcessBuilder.Create(settings);
			exporter.Run();
		}
	}
}