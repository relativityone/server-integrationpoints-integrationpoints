@Library('ProjectMayhem@v1') _
  
manualDeploy {
    slackChannel = "proj-rip-deployment" // Optional. Note: currently slack notifications are only sent on build failure
}