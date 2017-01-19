using System;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IServiceUrlRepository
	{
		InstanceUrlCollectionDTO RetrieveInstanceUrlCollection(Uri baseUri);
	}
}