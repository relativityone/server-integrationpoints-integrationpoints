@Library('ProjectMayhem@vServer') _

jobWithSut {
    slackChannel = "ci-server-delta"
    sutTemplate = "aio-server2022-rabbitmq"
    relativityBranch = "release-12.3-2023"
    jobScript = "Trident/Scripts/nightly-job.ps1"
    cron = "0 3 * * *"
}