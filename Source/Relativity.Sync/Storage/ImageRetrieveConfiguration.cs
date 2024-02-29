using Relativity.Sync.Configuration;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Storage
{
    internal class ImageRetrieveConfiguration : IImageRetrieveConfiguration
    {
        private readonly IConfiguration _cache;
        private readonly ISerializer _serializer;

        public ImageRetrieveConfiguration(IConfiguration cache, ISerializer serializer)
        {
            _cache = cache;
            _serializer = serializer;
        }

        public int[] ProductionImagePrecedence => _serializer.Deserialize<int[]>(_cache.GetFieldValue(x => x.ProductionImagePrecedence));

        public bool IncludeOriginalImageIfNotFoundInProductions => _cache.GetFieldValue(x => x.IncludeOriginalImages);
    }
}
