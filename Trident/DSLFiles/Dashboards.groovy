folder('DataTransfer-Jobs') {
}

folder('DataTransfer-Jobs/RelativitySync') {
}

multibranchPipelineJob('DataTransfer-Jobs/RelativitySync/UpdateSplunkDashboard') {
    factory {
        workflowBranchProjectFactory {
            scriptPath('Trident/Jobs/Dashboards.groovy')
        }
    }
    branchSources {
        branchSource {
            source {
                git {
                    remote('ssh://git@git.kcura.com:7999/dtx/relativitysync.git')
                    credentialsId('bitbucket-repo-key')
                    id('RelativitySync-Dashboards')
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
            includes('develop')
            excludes('')
        }
        traits << 'jenkins.plugins.git.traits.CleanAfterCheckoutTrait' {
            extension(class:'hudson.plugins.git.extensions.impl.CleanCheckout')
        }
    }
}
