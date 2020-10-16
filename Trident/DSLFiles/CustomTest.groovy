folder('DataTransfer-Jobs') {
}

folder('DataTransfer-Jobs/RelativitySync') {
}
 
pipelineJob("DataTransfer-Jobs/RelativitySync/CustomTest") {
    parameters {
        stringParam('Branch', '', 'The branch of this repository you want to checkout.')
        stringParam('SutTemplate', '', 'The SUT template to deploy, available templates - https://app.hopper.relativity.com/#/templates')
        stringParam('RelativityBranch', '', 'The branch of Relativity to upgrade to. Leave blank to skip Relativity upgrade.')
        stringParam('JobScript', 'Trident/Scripts/custom-test.ps1 -TestFilter "<FILTER>"', 'Tests you want to run after getting a SUT, e.g. All SystemTests: Trident/Scripts/Custom-Test.ps1 -TestFilter "namespace =~ Tests.System"')
        stringParam('RelativityVersionPath', '', 'Cannot be used with relativityBranch. The path for the version of Relativity to upgrade to on bld-pkgs. Bld-pkgs\\\\Packages or bld-pkgs\\\\Release can be used, but path must be the folder that contains the .exe, see https://einstein.kcura.com/x/8cH9D for more information.')
        stringParam('InvariantBranch', '', 'Cannot be used with invariantVersionPath. The branch of Invariant to upgrade to. Leave blank to skip Invariant upgrade.')
        stringParam('InvariantVersionPath', '', 'Cannot be used with invariantBranch. The path for the version of Invariant to upgrade to on bld-pkgs. Bld-pkgs\\\\Packages or bld-pkgs\\\\Release can be used, but path must be the folder that contains the .exe, see https://einstein.kcura.com/x/8cH9D for more information.')
        stringParam('CaatVersion', '', 'The version of CAAT to upgrade to. Leave blank to skip CAAT upgrade.')       
        booleanParam('EnableTestMode', false, 'Enable Test Mode if you need Audience. Testing Kepler endpoints to be hosted.')
        booleanParam('NotifyOnSuccess', false, 'This will send a slack message on each success of the pipeline.')
        stringParam('SlackChannel', 'sync_custom_test', 'The slack channel to notify on failure.')
    }
    definition {
        cpsScm {
            scm {
                git {
                    remote {
                        name('origin')
                        url('ssh://git@git.kcura.com:7999/dtx/relativitysync.git')
                        credentials('bitbucket-repo-key')
                    }
                    branch('*/${Branch}')
                    extensions {
                        cleanAfterCheckout()
                    }
                }
            }

            scriptPath('Trident/Jobs/CustomTest.groovy')
        }
    }
}