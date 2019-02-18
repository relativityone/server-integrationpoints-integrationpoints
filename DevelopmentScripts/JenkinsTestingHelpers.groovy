enum TestTypeT {
    integration,
    ui,
    integrationInQuarantine
}

@Field
def testStageNameT = [
	(TestType.integration) : "Integration Tests",
	(TestType.ui) : "UI Tests",
	(TestType.integrationInQuarantine) : "Integration Tests in Quarantine"
]

return this
