#!groovy

// Based on https://git.kcura.com/projects/RAID/repos/rmtjobs/browse/Template.jenkinsfile
// Set PipelineTools label to the same as here: https://git.kcura.com/projects/REL/repos/relativity/browse/Junkinsfile

@Library([ 'PipelineTools@RelativityCD-6.3.0'
         , 'GitHelpers@1.0.0'
         , 'SlackHelpers@1.0.0'
         ])_

properties([
    [$class: 'BuildDiscarderProperty', strategy: [$class: 'LogRotator', artifactDaysToKeepStr: '30', artifactNumToKeepStr: '', daysToKeepStr: '30', numToKeepStr: '']],
    parameters([
        string(defaultValue: 'develop', description: 'Set Relativity branch', name: 'relativityBranch'),
        string(defaultValue: 'DEV', description: 'Set Relativity build type (DEV, GOLD, etc.)', name: 'relativityBuildType'),
        string(defaultValue: '', description: 'Set Relativity version on which RIP will be tested. Leave blank to set the latest version.', name: 'relativityBuildVersion'),
        booleanParam(defaultValue: false, description: 'Check if you want to skip Integrations Tests stage.', name: 'skipIntegrationTests'),
        booleanParam(defaultValue: true, description: 'Check if you want to skip UI Tests stage.', name: 'skipUITests'),
        string(defaultValue: 'cat==SmokeTest', description: 'Set filter for integration and UI tests', name: 'testsFilter'),
        string(defaultValue: '', description: 'Set filter for nightly integration and UI tests', name: 'nightlyTestsFilter')
    ])
])

def nightlyJobName = "IntegrationPointsNightly"
def relativity_build = ""
// When RAID stage fails, verify if newer versions of cookboos exist
def ripCookbooks = '"relativity:= 4.1.12,role-testvm:= 3.12.0,role-ci:= 1.3.2,sql:= 2.4.1,servicebus:= 1.0.0"'

// Set this the same as in psake-test.ps1 in DevelopmentScripts
def integration_tests_results_file_path = "DevelopmentScripts/IntegrationTestsResults.xml"
def integration_tests_html_report = "IntegrationTestsResults.html"
def integration_tests_report_task = "generate_integration_tests_report"
def ui_tests_results_file_path = "DevelopmentScripts/UITestsResults.xml"
def ui_tests_html_report = "UITestsResults.html"
def ui_tests_report_task = "generate_ui_tests_report"
def server_name = ""

def numberOfFailedTests = -1
def numberOfPassedTests = -1
def numberOfSkippedTests = -1

def installing_relativity = true
def installing_invariant = false
def installing_analytics = false
def installing_datagrid = false

def agentsPool = "SCVMM-AGENTS-POOL"

// Do not modify.
def run_list = createRunList(installing_relativity, installing_invariant, installing_analytics, installing_datagrid)
def profile = createProfile(installing_relativity, installing_invariant, installing_analytics, installing_datagrid)
def knife = 'C:\\Python27\\Lib\\site-packages\\jeeves\\knife.rb'
def session_id = System.currentTimeMillis().toString()
def event_hash = java.security.MessageDigest.getInstance("MD5").digest(env.JOB_NAME.bytes).encodeHex().toString()

// Make changes here if necessary.
def python_packages = 'jeeves==4.1.0 phonograph==5.2.0 selenium==3.0.1'

// Pipeline
timeout(time: 3, unit: 'HOURS')
{
    timestamps 
    {
        catchError
        {
            node ('PolandBuild')
            {
                stage ('Checkout')
                {
                    retry(3)
                    {
                        timeout(time: 3, unit: 'MINUTES')
                        {
                            checkout scm
                            step([$class: 'StashNotifier', ignoreUnverifiedSSLPeer: true])
                        }
                    }
                }
                stage ('Build')
                {
                    retry(3)
                    {
                        timeout(time: 10, unit: 'MINUTES')
                        {
                            def sonarParameter = 
                                (env.BRANCH_NAME == "develop")
                                ? "-sonarqube"
                                : ""
                            powershell "./build.ps1 release $sonarParameter"
                            archiveArtifacts artifacts: "DevelopmentScripts/*.html", fingerprint: true
                        }
                    }
                }
                stage ('Unit Tests')
                {
                    timeout(time: 2, unit: 'MINUTES')
                    {
                        powershell "./build.ps1 release -sk -t"
                        archiveArtifacts artifacts: "TestLogs/*", fingerprint: true
                        currentBuild.result = 'SUCCESS'
                    }
                }
                stage ('Stash Tests Artifacts')
                {
                    stash includes: 'lib/UnitTests/**', name: 'testdlls'
                    stash includes: 'DynamicallyLoadedDLLs/Search-Standard/*', name: 'dynamicallyLoadedDLLs'
                    stash includes: 'Applications/RelativityIntegrationPoints.Auto.rap', name: 'integrationPointsRap'
                    stash includes: 'DevelopmentScripts/IntegrationPointsTests.*', name: 'nunitProjectFiles'
                    stash includes: 'DevelopmentScripts/NUnit.ConsoleRunner/tools/*', name: 'nunitConsoleRunner'
                    stash includes: 'DevelopmentScripts/NUnit.Extension.NUnitProjectLoader/tools/*', name: 'nunitProjectLoader'
                    stash includes: 'DevelopmentScripts/*.ps1', name: 'buildScripts'
                    stash includes: 'build.ps1', name: 'buildps1'
                    stash includes: 'Vendor/psake/tools/*', name: 'psake'
                    stash includes: 'Vendor/NuGet/NuGet.exe', name: 'nuget'
                    stash includes: 'Version/version.txt', name: 'version'
                }
                stage ('Cleanup Build Server')
                {
                    deleteDir()
                }
            }
            if (testingVMsAreRequired())
            {
                stage('Install RAID')
                {
                    node (agentsPool)
                    {
                        timeout(time: 45, unit: 'MINUTES')
                        {
                            // Pipelines can't do tuples, so I guess we'll do this the dumb way.https://issues.jenkins-ci.org/browse/JENKINS-45575
                            withCredentials([
                                usernamePassword(credentialsId: 'proget_ci', passwordVariable: 'PROGETPASSWORD', usernameVariable: 'PROGETUSERNAME'),
                                usernamePassword(credentialsId: 'SCVMM', passwordVariable: 'SCVMMPASSWORD', usernameVariable: 'SCVMMUSERNAME')])
                            {
                                echo "Getting server from pool, session_id: $session_id, Relativity branch: $params.relativityBranch, Relativity version: $params.relativityBuildVersion, Relativity build type: $params.relativityBuildType, event hash: $event_hash"
                                tuple = getServerFromPool(this, session_id, PROGETUSERNAME, PROGETPASSWORD, SCVMMUSERNAME, SCVMMPASSWORD)
                                server_name = tuple[0]
                                domain = tuple[1]
                                ip = tuple[2]
                                echo "Acquired server: $server_name.$domain ($ip)"
                            }

                            parallel (
                                Deploy:
                                {
                                    registerEvent(this, session_id, 'Talos_Provision_test_CD', 'PASS', '-c', "$server_name.$domain", profile, event_hash)
                                    if (installing_relativity)
                                    {
                                        if (!params.relativityBuildVersion)
                                        {
                                            relativity_build = getBuildArtifactsPath(this, "Relativity", params.relativityBranch, params.relativityBuildVersion, params.relativityBuildType, session_id)
                                            echo "Relativity version found: $relativity_build"
                                        }
                                        echo "Installed Relativity, branch: $params.relativityBranch, version: $relativity_build, type: $params.relativityBuildType"
                                        sendVersionToElastic(this, "Relativity", params.relativityBranch, relativity_build, params.relativityBuildType, session_id)
                                    }

                                    withCredentials([
                                        usernamePassword(credentialsId: 'proget_ci', passwordVariable: 'PROGETPASSWORD', usernameVariable: 'PROGETUSERNAME'),
                                        usernamePassword(credentialsId: 'cd_sut_svc', passwordVariable: 'SUTPASSWORD', usernameVariable: 'SUTUSERNAME'),
                                        usernamePassword(credentialsId: 'eddsdbo', passwordVariable: 'EDDSDBOPASSWORD', usernameVariable: 'EDDSDBOUSERNAME')])
                                    {
                                        deployments = [['product' : 'rel', 'build' : relativity_build, 'branch' : params.relativityBranch, 'type' : params.relativityBuildType]]
                                        attributeValues = makeAttributeValues(deployments, SUTUSERNAME, SUTPASSWORD, EDDSDBOPASSWORD)
                                        uploadEnvironmentFile(this, server_name, ripCookbooks, attributeValues, knife, session_id, PROGETUSERNAME, PROGETPASSWORD)
                                        addRunlist(this, session_id, server_name, ip, run_list, knife, SUTUSERNAME, SUTPASSWORD)
                                    }

                                    tags = getTags(this, server_name, knife, session_id)
                                    checkTags(deployments, tags)
                                    checkWorkspaceUpgrade(this, server_name, session_id)
                                },
                                ProvisionNodes:
                                {
                                    withCredentials([
                                        usernamePassword(credentialsId: 'proget_ci', passwordVariable: 'PROGETPASSWORD', usernameVariable: 'PROGETUSERNAME'),
                                        usernamePassword(credentialsId: 'SCVMM', passwordVariable: 'SCVMMPASSWORD', usernameVariable: 'SCVMMUSERNAME'),
                                        usernamePassword(credentialsId: 'cd_node_svc', passwordVariable: 'CDPASSWORD', usernameVariable: 'CDUSERNAME'),
                                        usernamePassword(credentialsId: 'Jenkins_sa', passwordVariable: 'JENKINSPASSWORD', usernameVariable: 'JENKINSUSERNAME'),
                                        usernamePassword(credentialsId: 'JenkinsSDLC', passwordVariable: 'SDLCPASSWORD', usernameVariable: 'SDLCUSERNAME')])
                                    {
                                        def numberOfSlaves = 2
                                        createNodes(this, session_id, numberOfSlaves, PROGETUSERNAME, PROGETPASSWORD, SCVMMUSERNAME, SCVMMPASSWORD, CDUSERNAME, CDPASSWORD, JENKINSUSERNAME, JENKINSPASSWORD)
                                        bootstrapDependencies(this, python_packages, params.relativityBranch, relativity_build, params.relativityBuildType, session_id, SDLCUSERNAME, SDLCPASSWORD)
                                    }
                                }
                            )
                        }
                    }
                }
                node ("$session_id && dependencies")
                {
                    stage ('Unstash Tests Artifacts')
                    {
                        unstash 'testdlls'
                        unstash 'dynamicallyLoadedDLLs'
                        unstash 'integrationPointsRap'
                        unstash 'nunitProjectFiles'
                        unstash 'nunitConsoleRunner'
                        unstash 'nunitProjectLoader'
                        unstash 'buildps1'
                        unstash 'buildScripts'
                        unstash 'psake'
                        unstash 'nuget'
                        unstash 'version'
                    }
                    catchError
                    {
                        stage ('Integration Tests')
                        {
                            timeout(time: 180, unit: 'MINUTES')
                            {
                                runTests(params.skipIntegrationTests, "-in", "Integration", nightlyJobName)
                            }
                        }
                        stage ('UI Tests')
                        {
                            timeout(time: 180, unit: 'MINUTES')
                            {
                                runTests(params.skipUITests, "-ui", "UI", nightlyJobName)
                            }
                        }
                        currentBuild.result = 'SUCCESS'
                    }
                    archiveTestsArtifacts(params.skipIntegrationTests, integration_tests_results_file_path, integration_tests_html_report, integration_tests_report_task)
                    numberOfFailedTests = getTestsStatistic(integration_tests_results_file_path, 'failed')
                    numberOfPassedTests = getTestsStatistic(integration_tests_results_file_path, 'passed')
                    numberOfSkippedTests = getTestsStatistic(integration_tests_results_file_path, 'skipped')
                    archiveTestsArtifacts(params.skipUITests, ui_tests_results_file_path, ui_tests_html_report, ui_tests_report_task)
                }
                stage('Cleanup VMs')
                {
                    try
                    {
                        timeout(time: 10, unit: 'MINUTES')
                        {
                            SaveVMs = false
                            // If we don't have a result, we didn't get to a test because somthing failed out earlier.
                            // If the result is FAILURE, a test failed.
                            if (!currentBuild.result || currentBuild.result == "FAILURE")
                            {
                                try
                                {
                                    timeout(time: 5, unit: 'MINUTES')
                                    {
                                        input(message: 'Save the VMs?', ok: 'Save', submitter: 'JNK-Basic')
                                    }
                                    SaveVMs = true
                                }
                                // This throws an error if you click abort or let it time out /shrug
                                catch(err)
                                {
                                    echo "VMs won't be saved."
                                }
                            }
                            withCredentials([
                                usernamePassword(credentialsId: 'proget_ci', passwordVariable: 'PROGETPASSWORD', usernameVariable: 'PROGETUSERNAME'),
                                usernamePassword(credentialsId: 'SCVMM', passwordVariable: 'SCVMMPASSWORD', usernameVariable: 'SCVMMUSERNAME')])
                            {
                                if (SaveVMs)
                                {
                                    saveVMs(this, session_id, PROGETUSERNAME, PROGETPASSWORD, SCVMMUSERNAME, SCVMMPASSWORD)
                                }
                                else
                                {
                                    deleteVMs(this, session_id, PROGETUSERNAME, PROGETPASSWORD, SCVMMUSERNAME, SCVMMPASSWORD)
                                }
                            }
                            deleteNodes(this, session_id)
                        }
                    }
                    catch (err)
                    {
                        echo "Cleanup VMs FAILED."
                    }
                }
            }
        }
        stage('Reporting')
        {
            try
            {
                echo "Build result: $currentBuild.result"
                node(agentsPool)
                {
                    timeout(time: 3, unit: 'MINUTES')
                    {
                        step([$class: 'StashNotifier', ignoreUnverifiedSSLPeer: true])
                        registerEvent(this, session_id, 'Pipeline_Status', currentBuild.result, '-ps', "$server_name.$domain", profile, event_hash, env.BUILD_URL)
                        withCredentials([usernamePassword(credentialsId: 'TeamCityUser', passwordVariable: 'TEAMCITYPASSWORD', usernameVariable: 'TEAMCITYUSERNAME')])
                        {
                            sendCDSlackNotification(this, env.BUILD_URL, (server_name ?: ""), relativity_build, env.BRANCH_NAME, params.relativityBuildType, getSlackChannelName(nightlyJobName).toString(), numberOfFailedTests as Integer, numberOfPassedTests as Integer, numberOfSkippedTests as Integer, TEAMCITYUSERNAME, TEAMCITYPASSWORD, currentBuild.result.toString()) 
                        }
                    }
                }
            }
            catch (err)  // Just catch everything here, if reporting/cleanup is the only thing that failed, let's not fail out the pipeline.
            {
                echo "Reporting failed: $err"
            }
        }
        
    }
}

// Helper functions
def configureNunitTests()
{
    withCredentials([usernamePassword(credentialsId: 'eddsdbo', passwordVariable: 'eddsdboPassword', usernameVariable: 'eddsdboUsername')])
    {
        def configuration_command = """python -m jeeves.create_config -t nunit -n "app.jeeves-ci" --dbuser "${eddsdboUsername}" --dbpass "${eddsdboPassword}" -s "${server_name}.kcura.corp" -db "${server_name}\\EDDSINSTANCE001" -o .\\lib\\UnitTests\\"""
        bat script: configuration_command
    }
 
}

def isNightly(String nightlyJobName)
{
    return env.JOB_NAME.contains(nightlyJobName)
}

def getSlackChannelName(String nightlyJobName)
{
    if (isNightly(nightlyJobName) && env.BRANCH_NAME == "develop")
    {
        return "#cd_rip_nightly"
    }
    else
    {
        return "#cd_rip_${env.BRANCH_NAME}"
    }
}

def getTestsFilter(String nightlyJobName)
{
    echo "env.JOB_NAME $env.JOB_NAME"
    return (isNightly(nightlyJobName))
        ? params.nightlyTestsFilter
        : params.testsFilter
}

def runTests(Boolean skipTests, String cmdOption, String name, String nightlyJobName)
{
    if (!skipTests) 
    {
        configureNunitTests()
        def currentFilter = getTestsFilter(nightlyJobName)
        def result = powershell returnStatus: true, script: "./build.ps1 -sk $cmdOption $currentFilter"
        if (result != 0)
        {
            error "$name Tests FAILED with status: $result"
        }
        echo "$name Tests OK"
    }
    else
    {
        echo "Skip $name Tests"
    }
}

def getTestsStatistic(String filePath, String prop)
{
    try
    {
        def cmd = ('''
            [xml]$testResults = Get-Content ''' 
            + filePath 
            + '''; $testResults.'test-run'.'''
            + "'$prop'")
        echo "getTestsStatistic cmd: $cmd"
        def stdout = powershell returnStdout: true, script: cmd
        echo "getTestsStatistic result: $stdout"
        if (stdout)
        {
            return stdout
        }
        else
        {
            return -1
        }
    }
    catch(err)
    {
        echo "getTestsStatistic error: $err"
        return -1
    }
}

def archiveTestsArtifacts(Boolean skipTests, String resultsFilePath, String reportFile, String generateHtmlReportTaskName)
{
    try
    {
        if (!skipTests) 
        {
            archiveArtifacts artifacts: "${resultsFilePath}", fingerprint: true
            powershell "Import-Module ./Vendor/psake/tools/psake.psm1; Invoke-psake ./DevelopmentScripts/psake-test.ps1 $generateHtmlReportTaskName"
            archiveArtifacts artifacts: "DevelopmentScripts/${reportFile}", fingerprint: true
        }
    }
    catch(err)
    {
        echo "archiveTestsArtifacts failed with error: $err"
    }
}

def testingVMsAreRequired()
{
    return !params.skipIntegrationTests || !params.skipUITests
}
