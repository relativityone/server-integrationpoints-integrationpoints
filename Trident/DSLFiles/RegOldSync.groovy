folder('IntegrationPoints-Jobs') {
}

folder('IntegrationPoints-Jobs/IntegrationPoints-Regression') {
    description('Prerequisites: "Functional Tests Template" workspace must exist; "rip.jenkins@rip.com" with default password must exist')
}

multibranchPipelineJob('IntegrationPoints-Jobs/IntegrationPoints-Regression/IntegrationPoints-Old-Sync') {
    factory {
        workflowBranchProjectFactory {
            scriptPath('Trident/Jobs/Reg-OldSync.groovy')
        }
    }
    branchSources {
        branchSource {
            source {
                git {
                    remote('ssh://git@git.kcura.com:7999/in/integrationpoints.git')
                    credentialsId('bitbucket-repo-key')
                    id('IntegrationPoints-Reg-Old-Sync')
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
            includes('develop release-*')
            excludes('')
        }
        traits << 'jenkins.plugins.git.traits.CleanAfterCheckoutTrait' {
            extension(class:'hudson.plugins.git.extensions.impl.CleanCheckout')
        }
    }
}
