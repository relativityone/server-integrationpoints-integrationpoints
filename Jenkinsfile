#!groovy

@Library('PipelineTools@master')_

properties ([
    parameters([
        choice(defaultValue: 'DEV', choices: ["DEV","GOLD"], description: 'Build type. GOLD can only be used on release branches.', name: 'buildType')
    ])
])

def version
def packageVersion

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
            withCredentials([usernamePassword(credentialsId: 'TCBuildVersionCredentials', passwordVariable: 'DBPASSWORD', usernameVariable: 'DBUSER')])
            {
                def outputString = powershell(returnStdout: true, script: ".\\build.ps1 getVersion -buildType ${params.buildType} -branchName ${env.BRANCH_NAME} -databaseUser $DBUSER -databasePassword $DBPASSWORD").trim()
                version = extractValue("VERSION", outputString)
                packageVersion = extractValue("PACKAGE_VERSION", outputString)
                if (!outputString || !version || !packageVersion)
                {
                    error("Unable to retrieve version!")
                }
                echo outputString
                currentBuild.displayName = packageVersion
            }            
        }
        stage ('ConfigureAwait check')
        {
            powershell ".\\build.ps1 checkConfigureAwait"
        }
        stage ('Build')
        {
            powershell ".\\build.ps1 buildAndSign -buildConfig Release -version $version -packageVersion $packageVersion"
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
            powershell ".\\build.ps1 packNuget -packageVersion $packageVersion"

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
            //TODO change slack to cd_sync after scripts development
            withCredentials([string(credentialsId: 'SlackJenkinsIntegrationToken', variable: 'token')])
            {
                message = "${env.BUILD_NUMBER} from ${env.BRANCH_NAME} failed.\n${env.BUILD_URL}"
                slackSend channel: "#cd_relativity-sync", color: "E8E8E8", message: "${message}", teamDomain: 'kcura-pd', token: token
            }

            if (env.BRANCH_NAME == 'develop' || env.BRANCH_NAME == 'master')
            {
                sendEmailAboutFailureToTeam()
            }
        }
    }
}

def sendEmailAboutFailureToTeam()
{
    def recipients = 'codigooplomo@relativity.com, buenavistacodingclub@relativity.com'
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

def extractValue(String value, String output)
{
    def matcher = output =~ "!!!$value=(.*)"
    $result =  matcher[0][0].split("=")[1]
    matcher = null
    return $result
}