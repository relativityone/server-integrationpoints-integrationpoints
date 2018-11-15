#!groovy

@Library('PipelineTools@master')_

properties ([
    parameters([
        choice(defaultValue: 'APLHA',choices: ["ALPHA","BETA","RC","GOLD"], description: 'Build Type', name: 'buildType'),
        string(defaultValue: '', description: 'Override Version, for example 1.2.3.4', name: 'overrideVersion')
    ])
])

def version

node('PolandBuild')
{
    try
    {
        stage ('Checkout')
        {
            scmProps = checkout scm
            step([$class: 'StashNotifier', ignoreUnverifiedSSLPeer: true, commitSha1: scmProps.GIT_COMMIT])
        }
        stage ('Get version')
        {
            if(params.overrideVersion != '')
            {
                version = params.overrideVersion
            }
            else
            {
                //TODO
                version = "0.0.0.1"
            }
        }
        stage ('ConfigureAwait check')
        {
            powershell ".\\build.ps1 checkConfigureAwait"
        }
        stage ('Build')
        {
            powershell ".\\build.ps1 buildAndSign -buildConfig Release -buildType '${params.buildType}' -version $version"
        }
        stage ('Unit Tests')
        {
            powershell ".\\build.ps1 runUnitTests"
        }
        stage ('Integration Tests')
        {
            powershell ".\\build.ps1 runIntegrationTests"
        }
        stage ('NuGet publish')
        {
            powershell ".\\build.ps1 packNuget -version $version"

            withCredentials([string(credentialsId: 'ProgetNugetApiKey', variable: 'key')])
            {
                powershell ".\\build.ps1 publishNuget -progetApiKey $key"
            }
        }
        if(env.BRANCH_NAME == 'develop')
		{
            stage ('SonarQube')
            {
                powershell ".\\build.ps1 runSonarScanner -version $version"
            }
            stage ('Security')
            {
                //TODO
            }
        }
        
        currentBuild.result = 'SUCCESS'
        step([$class: 'StashNotifier', ignoreUnverifiedSSLPeer: true])
    }
    catch (err)
    {
        currentBuild.result = 'FAILURE'
        echo err.toString()
        
        step([$class: 'StashNotifier', ignoreUnverifiedSSLPeer: true])
    }
    finally
    {
        if (currentBuild.result != 'SUCCESS')
        {
            send_slack_message(["#cd_relativity-sync"], "${env.BUILD_NUMBER} from ${env.BRANCH_NAME} failed.\n${env.BUILD_URL}", 'danger')

            if (env.BRANCH_NAME == 'develop' || env.BRANCH_NAME == 'master')
            {
                sendEmailAboutFailureToTeam()
            }
        }
    }
}

def sendEmailAboutFailureToTeam()
{
    // TODO
    def recipients = 'patryk.stepien@relativity.com'
    sendEmailAboutFailure(recipients)
}

def sendEmailAboutFailure(String recipients)
{
    def subject = "${env.JOB_NAME} - Build ${env.BUILD_DISPLAY_NAME} - Failed! On branch ${env.BRANCH_NAME}"
    def body = """${env.JOB_NAME} - Build - Failed:

Check console output at ${env.BUILD_URL} to view the results."""
    sendEmail(body, subject, recipients)
}

def sendEmail(String body, String subject, String recipients)
{
    emailext attachLog: true, body: body, subject: subject, to: recipients
}