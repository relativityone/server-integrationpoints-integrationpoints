using Relativity.Sync.Configuration;
using Relativity.Sync.Utils;
using System;
using System.Collections.Generic;

namespace Relativity.Sync.Storage
{
	internal class ImageRetrieveConfiguration : IImageRetrieveConfiguration
	{
		private readonly IConfiguration _cache;
		private readonly ISerializer _serializer;

		private static readonly Guid ProductionIdsGuid = new Guid();
		private static readonly Guid IncludeOriginalImageIfNotFoundInProductionsGuid = new Guid();

		public ImageRetrieveConfiguration(IConfiguration cache, ISerializer serializer)
		{
			_cache = cache;
			_serializer = serializer;
		}

		public int[] ProductionIds => _cache.GetFieldValue<int[]>(ProductionIdsGuid);

		public bool IncludeOriginalImageIfNotFoundInProductions => _cache.GetFieldValue<bool>(IncludeOriginalImageIfNotFoundInProductionsGuid);
	}
}
