namespace Relativity.Sync.Tests.Performance.PreConditions
{
	internal interface IPreCondition
	{
		bool Check();
		FixResult TryFix();
	}
}
