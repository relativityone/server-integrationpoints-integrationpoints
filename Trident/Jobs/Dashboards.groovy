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
            }
            stage ('Update Splunk dashboard')
            {
                dir('\\Trident\\Scripts')
                {
                    def secrets = [
                        [ secretType: 'Secret', name: 'FunctionAuthorizationKey', version: '', envVariable: 'FunctionAuthorizationKey' ]
                    ]

                    withAzureKeyvault(azureKeyVaultSecrets: secrets, keyVaultURLOverride: 'https://relativitysynckv.vault.azure.net/')
                    {
                        powershell './updateSplunkDashboard.ps1'
                    }
                }
            }
        }
        catch (err)
        {
            echo err.ToString()
            currentBuild.result = 'FAILED'
        }
    }