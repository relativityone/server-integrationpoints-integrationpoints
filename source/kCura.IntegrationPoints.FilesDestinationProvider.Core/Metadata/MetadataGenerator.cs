using System;
using kCura.IntegrationPoint.FilesDestinationProvider.Core.Files;
using kCura.IntegrationPoint.FilesDestinationProvider.Core.Metadata.Formatters;

namespace kCura.IntegrationPoint.FilesDestinationProvider.Core.Metadata
{
	public  class MetadataGenerator : IMetadaGenerator, IDisposable
	{
		private readonly IFileRepository _fileRepository;
		private readonly IMetadataFormatter _metadataFormatter;

		public MetadataGenerator(IFileRepository fileRepository, IMetadataFormatter metadataFormatter)
		{
			_fileRepository = fileRepository;
			_metadataFormatter = metadataFormatter;
		}

		public void Create(MetadataSettings settings)
		{
			_fileRepository.Create(settings.FilePath);
		}

		public void WriteHerader(MetadataSettings settings)
		{
			_fileRepository.Write(_metadataFormatter.GetHeaders(settings));
		}

		public void Dispose()
		{
			_fileRepository.Dispose();
		}
	}
}
