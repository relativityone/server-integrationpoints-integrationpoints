#!groovy

library 'SlackHelpers@6.0.0-Trident'

properties([
    [$class: 'BuildDiscarderProperty', strategy: [$class: 'LogRotator', artifactDaysToKeepStr: '30', artifactNumToKeepStr: '', daysToKeepStr: '30', numToKeepStr: '']],
    parameters([
        choice(choices: (env.BRANCH_NAME in ["master"]) ? 'GOLD\nDEV' : 'DEV\nGOLD', description: 'The type of build to install', name: 'type'),
        string(defaultValue: '', description: 'Setting the assembly version will override the default behavior of incrementing the counter on the versioning database.', name: 'assemblyVersionNumber'),
        string(defaultValue: '', description: 'Setting the installer version will override the default behavior of incrementing the counter on the versioning database.', name: 'installerVersionNumber')
    ])
])

def drive = ""
def jenkinsHelper = null
def hopperHelper = null
def commitHash = ""
def sign = (params.type == 'GOLD') ? '$true' : '$false'
def vmInfo = null
def unitTestResults = null
def runIntegrationTests = null
def integrationTestResults = null
def maxAttempts = 3 //maximum attempts to run the tests
def relativityHost = ""
def MaxBranchNameLength = 50

String buildOwner = ""
String buildIdentifier = "relativity-${UUID.randomUUID().toString()}"
String hopperApiUrl = "https://api.hopper.relativity.com/"
String hopperApiUsername = "homeimprovement@relativity.com"
String packagesPath = ""

def cleanupHangingTestProcesses() {
    // Chrome-related processes may have stuck around from UI tests.
    // We don't declare this in JenkinsHelpers.groovy because we want
    // to run it before we do git checkout.
    powershell "Get-WmiObject -Class Win32_Process -Filter \"name = 'chrome.exe' OR name = 'chromedriver.exe'\" | ForEach-Object { \$_.Terminate() }"
}

def sendSlackNotification(String relativityHost, String version, String branch, String type, String buildOwnerEmail, LinkedHashMap testsResults) {
    def hopperUrl = relativityHost ? "https://${relativityHost}/relativity" : ""
    sendCDSlackNotification(this, hopperUrl, version, branch, type, "#cd_${env.BRANCH_NAME}", buildOwnerEmail, testsResults, '', "prod")
}


node('role-build-agent')
    {
        try
        {
            stage ('Checkout')
            {
                def scmVars = checkout([
                    $class: 'GitSCM',
                    branches: scm.branches,
                    extensions: scm.extensions + [[$class: 'CloneOption', depth: 3, noTags: false, reference: '', shallow: false, timeout: 30], [$class: 'CleanBeforeCheckout']],
                    userRemoteConfigs: scm.userRemoteConfigs
                ])
            }
            stage ('Update dashboard')
            {
                dir('\\Trident\\Scripts')
                {
                    def secrets = [
                        [ secretType: 'Secret', name: 'FunctionAuthorizationKey', version: '', envVariable: 'FunctionAuthorizationKey' ]
                    ]

                    withAzureKeyvault(azureKeyVaultSecrets: secrets,
                        keyVaultURLOverride: 'https://relativitysynckv.vault.azure.net/')
                    {
                        powershell './updateSplunkDashboard.ps1'
                    }
                }
            }
        }
        catch (err)
        {
            echo err.ToString()
            currentBuild.result = 'FAILED'
        }
    }