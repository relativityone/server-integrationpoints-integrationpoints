folder('IntegrationPoints-Jobs') {
}

folder('IntegrationPoints-Jobs/IntegrationPoints-Nightly') {
}

multibranchPipelineJob('IntegrationPoints-Jobs/IntegrationPoints-Nightly/IntegrationPoints-UI-Old-Sync') {
    factory {
        workflowBranchProjectFactory {
            scriptPath('Trident/Jobs/UI-OldSync.groovy')
        }
    }
    branchSources {
        branchSource {
            source {
                git {
                    remote('ssh://git@git.kcura.com:7999/in/integrationpoints.git')
                    credentialsId('bitbucket-repo-key')
                    id('IntegrationPoints-UI-Old-Sync')
                }
            }
        }
    }
    orphanedItemStrategy {
        discardOldItems {
            numToKeep(20)
        }
    }
    configure {
        def traits = it / sources / data / 'jenkins.branch.BranchSource' / source / traits
        traits << 'jenkins.plugins.git.traits.BranchDiscoveryTrait'()
        traits << 'jenkins.scm.impl.trait.WildcardSCMHeadFilterTrait' {
            includes('*-test develop release-*')
            excludes('')
        }
        traits << 'jenkins.plugins.git.traits.CleanAfterCheckoutTrait' {
            extension(class:'hudson.plugins.git.extensions.impl.CleanCheckout')
        }
    }
}
