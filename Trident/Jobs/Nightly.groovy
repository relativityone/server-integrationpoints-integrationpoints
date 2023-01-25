@Library('ProjectMayhem@v1') _

jobWithSut {
    slackChannel = "ci-sync"
    sutTemplate = "aio-zarzaparrilla-latest"
    jobScript = "Trident/Scripts/nightly-job.ps1"
    cron = "0 3 * * *"
}