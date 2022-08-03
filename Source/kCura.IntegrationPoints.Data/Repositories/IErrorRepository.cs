using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
    public interface IErrorRepository
    {
        /// <summary>
        /// Creates errors in Relativity
        /// </summary>
        /// <param name="errors">The errors to create</param>
        void Create(IEnumerable<ErrorDTO> errors);
    }
}