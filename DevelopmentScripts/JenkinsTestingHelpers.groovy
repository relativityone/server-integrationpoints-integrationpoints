import groovy.transform.Field

@Field
enum TestTypeT {
    integration,
    ui,
    integrationInQuarantine
}

def testStageName(TestTypeT testType)
{
    if (testType == TestTypeT.integration)
    {
        return "Integration Tests"
    }
    if (testType == TestTypeT.integration)
    {
        return "Integration Tests"
    }
    if (testType == TestTypeT.ui)
    {
        return "UI Tests"
    }
    if (testType == TestTypeT.integrationInQuarantine)
    {
        return "Integration Tests in Quarantine"
    }
}

return this
