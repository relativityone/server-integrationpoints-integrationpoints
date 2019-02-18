import groovy.transform.Field

enum TestTypeT {
    integration,
    ui,
    integrationInQuarantine
}

@Field
def testStageNameT = [
	(TestTypeT.integration) : "Integration Tests",
	(TestTypeT.ui) : "UI Tests",
	(TestTypeT.integrationInQuarantine) : "Integration Tests in Quarantine"
]

return this
