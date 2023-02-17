using System.Collections.Generic;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
    public class ErrorManager : IErrorManager
    {
        private readonly IRepositoryFactory _repositoryFactory;

        internal ErrorManager(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public void Create(IEnumerable<ErrorDTO> errors)
        {
            IErrorRepository repository = _repositoryFactory.GetErrorRepository();

            repository.Create(errors);
        }
    }
}
