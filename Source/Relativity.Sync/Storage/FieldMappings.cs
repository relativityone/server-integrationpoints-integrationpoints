using System;
using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;

namespace Relativity.Sync.Storage
{
	internal sealed class FieldMappings : IFieldMappings
	{
		private List<FieldMap> _fieldMappings;

		private readonly IConfiguration _configuration;
		private readonly ISerializer _serializer;
		private readonly ISyncLog _logger;

		private static readonly Guid FieldMappingsGuid = new Guid("E3CB5C64-C726-47F8-9CB0-1391C5911628");

		public FieldMappings(IConfiguration configuration, ISerializer serializer, ISyncLog logger)
		{
			_configuration = configuration;
			_serializer = serializer;
			_logger = logger;
		}

		public IList<FieldMap> GetFieldMappings()
		{
			if (_fieldMappings != null)
			{
				return _fieldMappings;
			}

			string fieldMap = _configuration.GetFieldValue<string>(FieldMappingsGuid);

			try
			{
				_fieldMappings = _serializer.Deserialize<List<FieldMap>>(fieldMap);
				return _fieldMappings;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Unable to deserialize field mapping {fieldMap}.", fieldMap);
				throw;
			}
		}
	}
}