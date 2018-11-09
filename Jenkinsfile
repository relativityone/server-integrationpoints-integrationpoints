#!groovy

@Library('PipelineTools@master')

node('PolandBuild')
{
    try
    {
        stage ('Checkout')
        {
            checkout scm
            step([$class: 'StashNotifier', ignoreUnverifiedSSLPeer: true])
        }
    }
    catch (err)
    {
        currentBuild.result = 'FAILURE'
        step([$class: 'StashNotifier', ignoreUnverifiedSSLPeer: true])
        echo err.toString()

        if (env.BRANCH_NAME == 'develop' || env.BRANCH_NAME == 'master')
        {
            sendEmailAboutFailureToTeam()
        }
    }
    finally
    {
        if (currentBuild.result != 'SUCCESS')
        {
            send_slack_message(["#cd_relativity-sync"], "${env.BUILD_NUMBER} from ${env.BRANCH_NAME} failed.\n${env.BUILD_URL}", 'danger')
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