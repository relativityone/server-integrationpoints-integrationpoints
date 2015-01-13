using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Contracts
{
	//represents a wrapper to allow for certain safeties to be guaranteed when marshalling
	internal class ProviderWrapper : IDataSourceProvider
	{
		private readonly IDataSourceProvider _provider;
		internal ProviderWrapper(IDataSourceProvider provider)
		{
			if (provider == null)
			{
				throw new ArgumentNullException("provider");
			}
			_provider = provider;
		}

		public IEnumerable<FieldEntry> GetFields(string options)
		{
			try
			{
				//ToList(): http://stackoverflow.com/questions/2033998/passing-ienumerable-across-appdomain-boundaries
				return _provider.GetFields(options).ToList();
			}
			catch (Exception e)
			{
				throw GetNonCustomException(e);
			}
			
		}

		public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, string options)
		{
			try
			{
				return _provider.GetData(fields, entryIds, options);
			}
			catch (Exception e)
			{
				throw GetNonCustomException(e);
			}
			
		}

		public IDataReader GetBatchableIds(FieldEntry identifier, string options)
		{
			try
			{
				return _provider.GetBatchableIds(identifier, options);
			}
			catch (Exception e)
			{
				throw GetNonCustomException(e);
			}
			
		}

		//We want to make sure if there is a custom exception the code does not go crazy and blow up
		//more fail gracefully.
		private Exception GetNonCustomException(System.Exception ex)
		{
			var strBuilder = new StringBuilder();

			if (!String.IsNullOrWhiteSpace(ex.Message))
			{
				strBuilder.Append(ex.Message);
			}

			if (!String.IsNullOrEmpty(ex.StackTrace))
			{
				strBuilder.AppendLine(ex.StackTrace);
			}

			if (ex.InnerException != null)
			{
				strBuilder.AppendLine(ex.InnerException.ToString());
			}

			return new Exception(strBuilder.ToString());
		}
	}
}
