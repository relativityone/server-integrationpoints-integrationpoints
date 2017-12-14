using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Domain.Logging;

namespace kCura.IntegrationPoints.Core.Services.Domain
{
	public class ProviderWithLogContextDecorator : IDataSourceProvider
	{
		private readonly IDataSourceProvider _decoratedProvider;
		public ProviderWithLogContextDecorator(IDataSourceProvider decoratedProvider)
		{
			_decoratedProvider = decoratedProvider;
		}
		public IDataReader GetBatchableIds(FieldEntry identifier, string options)
		{
			IDataReader output;
			using (new SerilogContextRestorer())
			{
				output = _decoratedProvider.GetBatchableIds(identifier, options);
			}
			return new DataReaderWithLogContextDecorator(output);
		}

		public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, string options)
		{
			IDataReader output;
			using (new SerilogContextRestorer())
			{
				output = _decoratedProvider.GetData(fields, entryIds, options);
			}
			return new DataReaderWithLogContextDecorator(output);
		}

		public IEnumerable<FieldEntry> GetFields(string options)
		{
			using (new SerilogContextRestorer())
			{
				return _decoratedProvider.GetFields(options);
			}
		}
	}
}
