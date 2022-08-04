using System;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{

    /// <summary>
    /// This class extends ArtifactFieldDTO functionality by throw an exception during accessing time of the data
    /// If null exception pass in, Value will return an actual value of the class. 
    /// </summary>
    internal class LazyExceptArtifactFieldDto : ArtifactFieldDTO
    {
        private readonly Exception _exception;

        /// <summary>
        /// Takes exception to be stored during value accessing time.
        /// </summary>
        /// <param name="exception">The exception to be thrown.</param>
        public LazyExceptArtifactFieldDto(Exception exception)
        {
            _exception = exception;
        }

        public override object Value
        {
            get
            {
                if (_exception != null)
                {
                    throw _exception;
                }
                return base.Value;
            }
            set
            {
                base.Value = value;
            }
        }
    }
}