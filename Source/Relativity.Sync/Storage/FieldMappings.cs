using System;
using System.Collections.Generic;
using Relativity.API;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Storage
{
	internal sealed class FieldMappings : IFieldMappings
	{
		private List<FieldMap> _fieldMappings;

		private readonly IConfiguration _configuration;
		private readonly ISerializer _serializer;
		private readonly IAPILog _logger;

		public FieldMappings(IConfiguration configuration, ISerializer serializer, IAPILog logger)
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

			string fieldMap = _configuration.GetFieldValue(x => x.FieldsMapping);

			try
			{
				_fieldMappings = _serializer.Deserialize<List<FieldMap>>(fieldMap);
				return _fieldMappings;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Unable to deserialize field mapping.");
				throw;
			}
		}
	}
}
