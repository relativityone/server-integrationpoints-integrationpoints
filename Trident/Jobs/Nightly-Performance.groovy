@Library('ProjectMayhem@v1') _

jobWithSut {
    slackChannel = "ci-sync"
    sutTemplate = "aio-zarzaparrilla-latest"
    jobScript = "Trident/Scripts/nightly-performance.ps1"
    cron = "0 3 * * *"
}