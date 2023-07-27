@Library('ProjectMayhem@v1') _

jobWithSut {
    slackChannel = "ci-server-delta"
    sutTemplate = "aio-server2022-rabbitmq"
    jobScript = "Trident/Scripts/nightly-job.ps1"
    cron = "0 3 * * *"
}