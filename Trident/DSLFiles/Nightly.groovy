folder('DataTransfer-Jobs') {
}

folder('DataTransfer-Jobs/RelativitySync') {
}

multibranchPipelineJob('DataTransfer-Jobs/RelativitySync/Nightly') {
    factory {
        workflowBranchProjectFactory {
            scriptPath('Trident/Jobs/Nightly.groovy')
        }
    }
    branchSources {
        branchSource {
            source {
                git {
                    remote('ssh://git@git.kcura.com:7999/dtx/relativitysync.git')
                    credentialsId('bitbucket-repo-key')
                    id('RelativitySync-Nightly')
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
            includes('master develop release-*')
            excludes('')
        }
        traits << 'jenkins.plugins.git.traits.CleanAfterCheckoutTrait' {
            extension(class:'hudson.plugins.git.extensions.impl.CleanCheckout')
        }
    }
}
