using System.Collections.Generic;
using Relativity.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Core.Services
{
    public interface IFieldService
    {
	    List<FieldEntry> GetTextFields(int rdoTypeId, bool longTextFieldsOnly);
    }
}
