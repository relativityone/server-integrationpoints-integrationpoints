using System.Collections.Generic;
using Relativity.Sync.WorkspaceGenerator.Settings;

namespace Relativity.Sync.WorkspaceGenerator.Fields
{
    public interface IRandomFieldsGenerator
    {
        IEnumerable<CustomField> GetRandomFields(List<TestCase> testCases);
    }
}