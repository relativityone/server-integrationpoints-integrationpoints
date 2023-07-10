using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Agent.CustomProvider.Utils;
using Relativity.Import.V1.Models;

namespace kCura.IntegrationPoints.Agent.CustomProvider.DTO
{
    internal class ImportJobResult
    {
        public JobEndStatus Status { get; set; }

        public List<string> Errors { get; set; }

        public static ImportJobResult FromImportDetails(ImportDetails details)
        {
            List<string> errors = new List<string>();
            if (details?.Errors != null)
            {
                errors = details.Errors
                    .SelectMany(x => x.ErrorDetails)
                    .Select(x => x.ErrorMessage).ToList();
            }

            return new ImportJobResult
            {
                Status = details.State.ToJobEndStatus(),
                Errors = errors
            };
        }
    }
}
