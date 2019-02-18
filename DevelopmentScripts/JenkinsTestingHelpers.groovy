enum TestType {
    integration,
    ui,
    integrationInQuarantine
}

@Field
def testStageName = [
	(TestType.integration) : "Integration Tests",
	(TestType.ui) : "UI Tests",
	(TestType.integrationInQuarantine) : "Integration Tests in Quarantine"
]

return this
