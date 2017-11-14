using System;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.Queries
{
	public class CreateErrorRdoQuery
	{
		private const string _TRUNCATED_TEMPLATE = "(Truncated) {0}";
		private readonly IRsapiClientFactory _rsapiClientFactory;
		public const int MAX_ERROR_LEN = 2000;
		public const int MAX_SURCE_LEN = 255;

		public CreateErrorRdoQuery(IRsapiClientFactory rsapiClientFactory)
		{
			_rsapiClientFactory = rsapiClientFactory;
		}

		public virtual void Execute(Error errDto)
		{
			if (errDto.Message?.Length > MAX_ERROR_LEN)
			{
				errDto.Message = TruncateMessage(errDto.Message);
			}
			if (errDto.Source?.Length > MAX_SURCE_LEN)
			{
				errDto.Source = errDto.Source.Substring(0, MAX_SURCE_LEN);
			}
			using (IRSAPIClient client = _rsapiClientFactory.CreateAdminClient())
			{
				client.Repositories.Error.Create(errDto);
			}
		}

		private string TruncateMessage(string message)
		{
			int truncatedLength = MAX_ERROR_LEN - _TRUNCATED_TEMPLATE.Length;
			return string.Format(_TRUNCATED_TEMPLATE, message.Substring(0, truncatedLength));
		}
	}
}