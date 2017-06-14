using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.EventHandler;
using System.Runtime.InteropServices;
using System.Security.Claims;
using kCura.EventHandler.CustomAttributes;
using Relativity.API;
using Relativity.Core.Service;
using kCura.IntegrationPoints.Data.Extensions;
using Relativity.Core;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
    [Guid("51BAAFB6-898F-48AC-BB6E-A5B9FCD4EF1F")]
    [Description("This is an event handler that removes Integration Points Resource Files.")]
    [RunTarget(EventHandler.Helper.RunTargets.Instance)]
    [RunOnce(true)]
    public class RemoveResourceFileEventHandler : PreInstallEventHandler
    {
        private string[] assemblyNames =
        {
            "kCura.Image.Viewer.dll",
            "kCura.Print.dll"
        };

        public override Response Execute()
        {
            Guid applicationGuid = new Guid(Domain.Constants.IntegrationPoints.APPLICATION_GUID_STRING);

            BaseServiceContext baseServiceContext = ClaimsPrincipal.Current.GetUnversionContext(-1);
            try
            {
                AssemblyManager am = new AssemblyManager();

                foreach (var assemblyName in assemblyNames)
                {
                    var assembly = am.Read(baseServiceContext, assemblyName, applicationGuid);
                    int assemblyArtifactId = assembly.ArtifactID;

                    if (assemblyArtifactId > 0)
                    {
                        am.Delete(baseServiceContext, assemblyArtifactId);
                    }
                }
            }
            catch (Exception ex)
            {
                return new Response
                {
                    Exception = ex,
                    Message = ex.Message,
                    Success = false
                };
            }

            return new Response
            {
                Message = "Resource Files were removed successfully",
                Success = true
            };
        }
    }
}
