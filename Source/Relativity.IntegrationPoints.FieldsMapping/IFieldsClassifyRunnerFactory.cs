namespace Relativity.IntegrationPoints.FieldsMapping
{
	public interface IFieldsClassifyRunnerFactory
	{
		IFieldsClassifierRunner CreateForSourceWorkspace();
		IFieldsClassifierRunner CreateForDestinationWorkspace();
	}
}
