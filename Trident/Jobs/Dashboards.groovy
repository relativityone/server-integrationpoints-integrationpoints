#!groovy

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

                dir('\\Trident\\Scripts')
                {
                    jenkinsHelper = load pwd() + '/JenkinsHelpers.groovy'
                    hopperHelper = load pwd() + '/HopperHelpers.groovy'
                    def secrets = [
                        [ secretType: 'Secret', name: 'FunctionAuthorizationKey', version: '', envVariable: 'FunctionAuthorizationKey' ]
                    ]

                    withAzureKeyvault(azureKeyVaultSecrets: secrets,
                        keyVaultURLOverride: 'https://relativitysynckv.vault.azure.net/')
                    {
                        buildOwner = hopperHelper.getBuildOwner(bbUsername, bbPassword)
                    }
                }
            }
        }
    }