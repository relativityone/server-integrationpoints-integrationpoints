using System;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class HardCodedServiceUrlRepository : IServiceUrlRepository
	{
		public InstanceUrlCollectionDTO RetrieveInstanceUrlCollection(Uri baseUri)
		{
			Uri baseUriWithoutRelativity = new Uri(baseUri.GetLeftPart(UriPartial.Authority));
			Uri keplerUri;
			Uri rsapiUri;
			Uri webApiUri;
			Uri.TryCreate(baseUriWithoutRelativity, "Relativity.Services", out rsapiUri);
			Uri.TryCreate(baseUriWithoutRelativity, "Relativity.REST/api/", out keplerUri);
			Uri.TryCreate(baseUriWithoutRelativity, "RelativityWebAPI/", out webApiUri);

			var instanceUrlCollection = new InstanceUrlCollectionDTO()
			{
				InstanceUrl = baseUri.AbsoluteUri,
				RsapiUrl = rsapiUri.AbsoluteUri,
				KeplerUrl = keplerUri.AbsoluteUri,
				WebApiUrl = webApiUri.AbsoluteUri
			};

			return instanceUrlCollection;
		}
	}
}