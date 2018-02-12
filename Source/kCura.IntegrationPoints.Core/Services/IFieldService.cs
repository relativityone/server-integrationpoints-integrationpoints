using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Core.Services
{
    public interface IFieldService
    {
	    List<FieldEntry> GetTextFields(int rdoTypeId, bool longTextFieldsOnly);
    }
}
