using Relativity.Sync.Configuration;
using Relativity.Sync.Utils;
using System;
using Relativity.Sync.RDOs;

namespace Relativity.Sync.Storage
{
	internal class ImageRetrieveConfiguration : IImageRetrieveConfiguration
	{
		private readonly IConfiguration _cache;
		private readonly ISerializer _serializer;

		private static readonly Guid ProductionImagePrecedenceGuid = new Guid("421CF05E-BAB4-4455-A9CA-FA83D686B5ED");

		public ImageRetrieveConfiguration(IConfiguration cache, ISerializer serializer)
		{
			_cache = cache;
			_serializer = serializer;
		}

		public int[] ProductionImagePrecedence => _serializer.Deserialize<int[]>(_cache.GetFieldValue<string>(ProductionImagePrecedenceGuid));

		public bool IncludeOriginalImageIfNotFoundInProductions => _cache.GetFieldValue<bool>(SyncConfigurationRdo.IncludeOriginalImagesGuid);
	}
}
