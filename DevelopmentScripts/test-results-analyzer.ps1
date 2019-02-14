function store_tests_results() 
{
    $body = @{
		FullTestName = "kCura.IntegrationPoints.EventHandlers.Tests.Integration.Installers.SecretStoreCleanUpTests.ItShouldRemoveSecretAndTenantId"
		Result = "Passed"
		BranchId = "developnightly"
		BuildName = "DEV-102TESDEPLOYED"
		TestType = "IntegrationInQuarantine"
		Duration = 30.111123
		Categories = @( "SmokeTest", "InQuarantine" )
		Message = 'at kCura.Data.RowDataGateway.Context.ExecuteSqlStatementAsList[T](String sqlStatement, Func`2 converter, IEnumerable`1 parameters, Int32 timeoutValue) at Relativity.SecretCatalog.SQL.SQLCatalog.SecretManager.ReadTenantSecrets(String tenantID) at Relativity.APIHelper.SecretStore.SecretStoreSecretCatalog.List(String path) at Relativity.APIHelper.SecretStore.SecretStoreSecretCatalog.ListAsync(String path) at Relativity.Core.SecretCatalogFactory.VB$StateMachine_12_GetTenantSecretPathsAsync.MoveNext() --- End of stack trace from previous location where exception was thrown --- at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw() at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task) at Relativity.Core.SecretCatalogFactory.VB$StateMachine_11_GetTenantSecretsAsync.MoveNext() --- End of stack trace from previous location where exception was thrown --- at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw() at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task) at Relativity.Core.SecretCatalogFactory.GetTenantSecrets(String tenantID) at kCura.IntegrationPoints.EventHandlers.Tests.Integration.Installers.SecretStoreCleanUpTests.ItShouldRemoveSecretAndTenantId'
	} | ConvertTo-Json

    $wc =Invoke-WebRequest -Uri "https://testresultsanalyzer.azurewebsites.net/api/StoreTestResultFunction?code=BqgMKu1Mp/WMNPKfacvIGR4oSzBpDGgdxKNGYnAOOPrwe9DHYQidlA==" -ContentType "application/json" -Method POST -Body $body
}