namespace Relativity.IntegrationPoints.FieldsMapping
{
    public interface IFieldsClassifyRunnerFactory
    {
        IFieldsClassifierRunner CreateForSourceWorkspace(int artifactTypeId);
        IFieldsClassifierRunner CreateForDestinationWorkspace(int artifactTypeId);
    }
}
