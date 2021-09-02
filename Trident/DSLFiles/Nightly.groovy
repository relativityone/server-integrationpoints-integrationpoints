folder('IntegrationPoints-Jobs/IntegrationPoints-Nightly') {
}

multibranchPipelineJob('IntegrationPoints-Jobs/IntegrationPoints-Nightly/IntegrationPoints-Nightly') {
    factory {
        workflowBranchProjectFactory {
            scriptPath('Trident/Jobs/Nightly.groovy')
        }
    }
    branchSources {
        branchSource {
            source {
                git {
                    remote('ssh://git@git.kcura.com:7999/in/integrationpoints.git')
                    credentialsId('bitbucket-repo-key')
                    id('IntegrationPoints-Nightly')
                }
            }
        }
    }
    orphanedItemStrategy {
        discardOldItems {
            daysToKeep(1)
            numToKeep(10)
        }
    }
    configure {
        def traits = it / sources / data / 'jenkins.branch.BranchSource' / source / traits
        traits << 'jenkins.plugins.git.traits.BranchDiscoveryTrait'()
        traits << 'jenkins.scm.impl.trait.WildcardSCMHeadFilterTrait' {
            includes('REL-488513-webapi-deprecation develop master')
        }
        traits << 'jenkins.plugins.git.traits.CleanAfterCheckoutTrait' {
            extension(class:'hudson.plugins.git.extensions.impl.CleanCheckout')
        }

        def namedBranchStrategies = it / sources / data / 'jenkins.branch.BranchSource' / buildStrategies / 'jenkins.branch.buildstrategies.basic.AllBranchBuildStrategyImpl' / 'strategies'
        namedBranchStrategies / 'jenkins.branch.buildstrategies.basic.SkipInitialBuildOnFirstBranchIndexing'
    }
}
