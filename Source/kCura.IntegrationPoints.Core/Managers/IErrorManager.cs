using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Managers
{
    public interface IErrorManager
    {
        /// <summary>
        /// Creates Relativity errors.
        /// </summary>
        /// <param name="errors">The errors to create.</param>
        void Create(IEnumerable<ErrorDTO> errors);
    }
}
