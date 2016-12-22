//region HELP!!!
/*
General Jenkinsfile pipeline guide: 
https://einstein.kcura.com/display/TAL/How+to+Create+a+CD+Pipeline+using+Jenkinsfiles

I don't want to install something, or I want to install something else.
https://einstein.kcura.com/display/TAL/Selecting+What+Gets+Installed

I don't want to test something, or I want to test something else.
https://einstein.kcura.com/display/TAL/Running+Tests

How does reporting work?
https://einstein.kcura.com/display/TAL/Pipeline+Reporting
*/
//endregion

type = 'DEV'
build = "NULL"
invariantbuild = "NULL"
installing_relativity = 'true'
installing_analytics = 'true'
installing_invariant = 'true'
installing_datagrid = 'true'


//region GlobalVariables
installing_relativity = (installing_relativity == 'true') ? true : false
installing_analytics = (installing_analytics == 'true') ? true : false
installing_invariant = (installing_invariant == 'true') ? true : false
installing_datagrid = (installing_datagrid == 'true') ? true : false
def components = [installing_relativity, installing_analytics, installing_invariant, installing_datagrid]

def services = []
def run_list = ""

def profile = "profile"
if (components.every()) {
    profile = "profile_all"
    services = ['kCura EDDS Agent Manager', 'kCura EDDS Web Processing Manager', 'kCura Service Host Manager', 'QueueManager']
    run_list = "caServerSetup::default,installRelAll::default,updateConfig::default,installInvAll::default,installAnalyticsAll::default,installDataGrid::default,installDataGrid::updateRelativitySQL,installInvAll::invariantEndpoint"
} else {
    if (installing_relativity) {
        profile += "_rel"
        services.add('kCura EDDS Agent Manager')
        services.add('kCura EDDS Web Processing Manager')
        services.add('kCura Service Host Manager')
        if (installing_analytics) {
            run_list += ",caServerSetup::default"
        }
        run_list += ",installRelAll::default,updateConfig::default"
    }
    if (installing_analytics) {
        profile += "_ana"
        if (!installing_analytics) {
            run_list += ",caServerSetup::default"
        }
        run_list += ",installAnalyticsAll::default"
    }
    if (installing_invariant) {
        profile += "_inv"
        services.add('QueueManager')
        run_list += ",installInvAll::default,installInvAll::invariantEndpoint"
    }
    if (installing_datagrid) {
        profile += "_dg"
        run_list += ",installDataGrid::default,installDataGrid::updateRelativitySQL"
    }
    run_list = run_list[1..-1]
}

def relativity_branch = 'develop'
def automation_branch = 'develop'
def invariant_branch = 'develop'

def passed = false
def status = "FAIL"

def random_server = ""
def server_name = ""
def domain = ""

def names_of_slaves = []
def number_of_slaves = '2'

def session_id = System.currentTimeMillis().toString()

// The event_hash is created by using the relativity branch, + the invariant branch, + the job name.
def s = relativity_branch + invariant_branch + env.JOB_NAME
def event_hash = java.security.MessageDigest.getInstance("MD5").digest(s.bytes).encodeHex().toString()
//endregion

stage('Get Server') {
	def file_name = UUID.randomUUID().toString() + ".txt"
	def windows_path = $/\\dv-file-01.testing.corp\Testing\TestingData\PooledServers\/$ + file_name
	def linux_path = "/mnt/dv-file-01.testing.corp/TestingData/PooledServers/" + file_name

	build job: 'Provision.VMware.GetServerFromPool', parameters: [
		[$class: 'NodeParameterValue', name: 'node_label', labels: ['chef'], nodeEligibility: [$class: 'AllNodeEligibility']],
		string(name: 'temp_file', value: windows_path),
		string(name: 'pool_name', value: 'cd')]
					
	def file = new File(linux_path)
	random_server = file.text

	def file_deleted = file.delete()
		
	echo "*************************************************" +
	"\nProfile:" + profile + 
	"\nRun list:" + run_list +
	"\nInstalling Relativity to " + random_server +
	"\nBranch:" + relativity_branch +
	"\nBuild:" + build +
	"\nType:" + type +
	"\nsession_id = " + session_id +
	"\nevent_hash: " + event_hash +
	"\n*************************************************"
		
	def tokens = random_server.tokenize(".")
	server_name = tokens[0]
	domain = tokens[1] + '.' + tokens[2]
	
	for (i = 0; i < number_of_slaves.toInteger(); i++) {
		names_of_slaves.add("JNK-" + server_name +  "-" + i)
	}
}

// WTF Java has a 64kb limit for any function that it runs.
// This means that the number of steps that we can run is limited.
// https://issues.jenkins-ci.org/browse/JENKINS-37984
def build_tests(String server_name, String domain, String session_id, String relativity_branch, String automation_branch, Boolean installing_invariant, Boolean installing_datagrid) {
    try {
    	parallel (
			//RIP_Integration_Tests: {build job: 'Parameterized.NUnit', parameters: [
    		//	[$class: 'NodeParameterValue', name: 'node_label', labels: [server_name], nodeEligibility: [$class: 'AllNodeEligibility']],
    		//	string(name: 'session_id', value: session_id),
    		//	string(name: 'SERVER', value: server_name),
    		//	string(name: 'DOMAIN', value: domain),
    		//	string(name: 'branch', value: env.BRANCH_NAME),
    		//	string(name: 'assembly', value: 'lib\\UnitTests\\kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration'),
    		//	string(name: 'tests_to_skip', value: '0'),
    		//	string(name: 'repository', value: 'integrationpoints')]},
    		RIP_System_Tests: {build job: 'test.Parameterized.Robot', parameters: [
    			[$class: 'NodeParameterValue', name: 'node_label', labels: [server_name], nodeEligibility: [$class: 'AllNodeEligibility']],
    			string(name: 'session_id', value: session_id),
    			string(name: 'SERVER', value: server_name),
    			string(name: 'DOMAIN', value: domain),
    			string(name: 'RelativityBuild', value: '"NULL"'),
    			string(name: 'RelativityType', value: '"NULL"'),
    			string(name: 'branch', value: automation_branch),
    			string(name: 'config_file', value: 'Config/smokeTests.cfg'),
    			string(name: 'SUITE', value: 'Relativity.Applications.RelativityIntegrationPoints.SmokeTests')]}
    	)
    } finally {
	    // These jobs still delete the lib folder at startup, any robot tests running after will break :(
    	parallel(
    	    Productions_NUnit: {build job: 'Parameterized.NUnit.Category', parameters: [
    			[$class: 'NodeParameterValue', name: 'node_label', labels: [server_name], nodeEligibility: [$class: 'AllNodeEligibility']],
    			string(name: 'session_id', value: session_id),
    			string(name: 'SERVER', value: server_name),
    			string(name: 'DOMAIN', value: domain),
    			string(name: 'branch', value: 'develop'),
    			string(name: 'assembly', value: 'Relativity.Productions.NUnit.Integration'),
    			string(name: 'category', value: 'testtype.ci'),
    			string(name: 'repository', value: 'Productions')]},
    		Security_Automated_Tests: {build job: 'Parameterized.NUnit', parameters: [
    			[$class: 'NodeParameterValue', name: 'node_label', labels: [server_name], nodeEligibility: [$class: 'AllNodeEligibility']],
    			string(name: 'session_id', value: session_id),
    			string(name: 'SERVER', value: server_name),
    			string(name: 'DOMAIN', value: domain),
    			string(name: 'branch', value: 'master'),
    			string(name: 'assembly', value: 'Security.NUnit.IntegrationTests'),
    			string(name: 'tests_to_skip', value: '2'),
    			string(name: 'repository', value: 'SecurityAutomatedTests')]},
    		Conversion: {build job: 'Parameterized.NUnit.Category', parameters: [
    			[$class: 'NodeParameterValue', name: 'node_label', labels: [server_name], nodeEligibility: [$class: 'AllNodeEligibility']],
    			string(name: 'session_id', value: session_id),
    			string(name: 'SERVER', value: server_name),
    			string(name: 'DOMAIN', value: domain),
    			string(name: 'branch', value: relativity_branch),
    			string(name: 'assembly', value: 'Relativity.Conversion.NUnit.Integration'),
    			string(name: 'category', value: 'testtype.ci'),
    			string(name: 'repository', value: 'Relativity')]}
        )
    }
}

try {
    stage('Install RAID') {
    	parallel Deploy: {
    		build job: "Reporting.RegisterEvent", parameters: [
    			[$class: 'NodeParameterValue', name: 'node_label', labels: ['chef'], nodeEligibility: [$class: 'AllNodeEligibility']],
    			string(name: 'action', value: '-c'), 
    			string(name: 'session_id', value: session_id), 
    			string(name: 'deployment_step', value: 'Talos_Provision_test_CD'), 
    			string(name: 'host', value: random_server), 
    			string(name: 'profile', value: profile), 
    			string(name: 'event_hash', value: event_hash)]
    						
    		build job: 'Provision.Chef.UpdateInstallers', parameters: [
    			string(name: 'type', value: type), 
    			string(name: 'build', value: build), 
    			string(name: 'branch', value: relativity_branch), 
    			string(name: 'invariantbuild', value: invariantbuild), 
    			string(name: 'invariantbranch', value: invariant_branch),
    			string(name: 'session_id', value: session_id), 
    			string(name: 'server', value: server_name),
    			[$class: 'NodeParameterValue', name: 'node_label', labels: [server_name + ' || chef'], nodeEligibility: [$class: 'AllNodeEligibility']]]
    			
    		build job: 'Provision.Chef.AddRunList', parameters: [
    			string(name: 'node', value: random_server), 
    			string(name: 'recipes', value: run_list), 
    			string(name: 'session_id', value: session_id),  
    			[$class: 'NodeParameterValue', name: 'node_label', labels: [server_name + ' || chef'], nodeEligibility: [$class: 'AllNodeEligibility']]]
    		
    		if (installing_relativity) {
    			build job: 'Provision.Chef.WaitForTags', parameters: [
        			string(name: 'node', value: random_server), 
        			string(name: 'tags', value: '-t '  + server_name + '-psrel '
        									  + '-t '  + server_name + '-relativityInstalled '
        									  + '-t '  + server_name + '-upcfg'), 
        			string(name: 'session_id', value: session_id),
        			string(name: 'deployment_step', value: 'Relativity_Installation'),
        			string(name: 'component', value: 'relativity'),
        			[$class: 'NodeParameterValue', name: 'node_label', labels: [server_name], nodeEligibility: [$class: 'AllNodeEligibility']]]
    		}
    		
    	    if (installing_invariant) {
        		build job: 'Provision.Chef.WaitForTags', parameters: [
        			string(name: 'node', value: random_server), 
                    string(name: 'tags', value: '-t '  + server_name + '-invariantInstalled '
        			    					  + '-t '  + server_name + '-isinv '
        				    				  + '-t '  + server_name + '-iqinv'),
        			string(name: 'session_id', value: session_id),
        			string(name: 'deployment_step', value: 'Invariant_Installation'),
        			string(name: 'component', value: 'invariant'),
        			[$class: 'NodeParameterValue', name: 'node_label', labels: [server_name], nodeEligibility: [$class: 'AllNodeEligibility']]]
            }
    
            if (installing_analytics) {
        		build job: 'Provision.Chef.WaitForTags', parameters: [
            		string(name: 'node', value: random_server), 
            		string(name: 'tags', value: '-t '  + server_name + '-analyticsInstalled'), 
            		string(name: 'session_id', value: session_id),
            		string(name: 'deployment_step', value: 'Analytics_Installation'),
            		string(name: 'component', value: 'analytics'),
            		[$class: 'NodeParameterValue', name: 'node_label', labels: [server_name], nodeEligibility: [$class: 'AllNodeEligibility']]]
            }
    
            if (installing_datagrid) {
        		build job: 'Provision.Chef.WaitForTags', parameters: [
            		string(name: 'node', value: random_server), 
            		string(name: 'tags', value: '-t '  + server_name + '-datagridInstalled '
            		                          + '-t '  + server_name + '-datagridDone'), 
            		string(name: 'session_id', value: session_id),
            		string(name: 'deployment_step', value: 'DataGrid_Installation'),
            		string(name: 'component', value: 'datagrid'),
            		[$class: 'NodeParameterValue', name: 'node_label', labels: [server_name], nodeEligibility: [$class: 'AllNodeEligibility']]]
            }
    
    	}, ProvisionNodes: {
    		build job: "Provision.VMware.ExpiryAttribute", parameters: [
    			[$class: 'NodeParameterValue', name: 'node_label', labels: ['chef'], nodeEligibility: [$class: 'AllNodeEligibility']],
    			string(name: 'node', value: random_server), 
    			string(name: 'hours', value: '12')]
    			
    		build job: "vCenter.create_jenkins_slaves",	parameters: [
    		[$class: 'NodeParameterValue', name: 'node_label', labels: ['chef'], nodeEligibility: [$class: 'AllNodeEligibility']],
    			string(name: 'TemplateName', value: 'TT-Win7-IE10'), 
    			string(name: 'VMNames', value: names_of_slaves.toString()), 
    			string(name: 'StartSequence', value: '1'), 
    			string(name: 'NumberOfSlaves', value: number_of_slaves), 
    			string(name: 'JenkinsServer', value: 'POLAND'), 
    			string(name: 'Label', value: server_name)]
    			
    		for (name_of_slave in names_of_slaves){
    			build job: 'Nodes.PullTestDependencies', parameters: [
    			    [$class: 'NodeParameterValue', name: 'node_label', labels: [name_of_slave], nodeEligibility: [$class: 'AllNodeEligibility']],
    				string(name: 'RelativityBuild', value: build),
    				string(name: 'RelativityType', value: type), 
    				string(name: 'branch', value: relativity_branch)],
    				wait: false
    		}
    	}, failFast: true
    }

	stage('Deployment Validation') {
		build job: 'Provision.Chef.DeleteChef',	parameters: [
			[$class: 'NodeParameterValue', name: 'node_label', labels: [server_name], nodeEligibility: [$class: 'AllNodeEligibility']],
			string(name: 'node', value: random_server)]
			
		services_to_check = '"'
		for (service in services) {
		    services_to_check += random_server + ';TESTING\\rellockdown;P@ssw0rd@1;' + service + ','
		}
		services_to_check = services_to_check[0..-2] + '"'

		build job: "Provision.ValidateDeployment", parameters: [
			[$class: 'NodeParameterValue', name: 'node_label', labels: [server_name], nodeEligibility: [$class: 'AllNodeEligibility']],
			string(name: 'environment', value: random_server),
			string(name: 'server_ips', value: random_server),
			string(name: 'job_name', value: env.JOB_NAME),
			string(name: 'build_num', value: env.BUILD_NUMBER),
			string(name: 'UseWIX', value: '--wix'),
			string(name: 'to', value: 'pletang@kcura.com'),
			string(name: 'services', value: services_to_check),
			string(name: 'session_id', value: session_id)]
	}

	stage('Post Install Setup') {
	    if (installing_relativity) {
            build job: 'test.Parameterized.Robot', parameters: [
    			[$class: 'NodeParameterValue', name: 'node_label', labels: [server_name], nodeEligibility: [$class: 'AllNodeEligibility']],
    			string(name: 'session_id', value: session_id),
    			string(name: 'SERVER', value: server_name),
    			string(name: 'DOMAIN', value: domain),
    			string(name: 'RelativityBuild', value: '"NULL"'),
    			string(name: 'RelativityType', value: '"NULL"'),
    			string(name: 'branch', value: 'tag_setup_tests'),
    			string(name: 'username', value: 'relativity.admin@kcura.com'),
    			string(name: 'SUITE', value: 'Relativity.RelativitySetup')], propagate: false
				
			build job: 'test.Parameterized.Robot', parameters: [
    			[$class: 'NodeParameterValue', name: 'node_label', labels: [server_name], nodeEligibility: [$class: 'AllNodeEligibility']],
    			string(name: 'session_id', value: session_id),
    			string(name: 'SERVER', value: server_name),
    			string(name: 'DOMAIN', value: domain),
    			string(name: 'RelativityBuild', value: '"NULL"'),
    			string(name: 'RelativityType', value: '"NULL"'),
    			string(name: 'branch', value: automation_branch),
    			string(name: 'config_file', value: 'Config/smokeTests.cfg'),
				string(name: 'extra_args', value: '--variable BRANCH='+env.BRANCH_NAME),
    			string(name: 'SUITE', value: 'Automation.PostInstall.InstallApps.UpdateRIPApp')], propagate: false	
	    }
	    
	    if (installing_invariant) {
            build job: 'test.Parameterized.Robot', parameters: [
    			[$class: 'NodeParameterValue', name: 'node_label', labels: [server_name], nodeEligibility: [$class: 'AllNodeEligibility']],
    			string(name: 'session_id', value: session_id),
    			string(name: 'SERVER', value: server_name),
    			string(name: 'DOMAIN', value: domain),
    			string(name: 'RelativityBuild', value: '"NULL"'),
    			string(name: 'RelativityType', value: '"NULL"'),
    			string(name: 'branch', value: 'tag_setup_tests'),
    			string(name: 'username', value: 'relativity.admin@kcura.com'),
    			string(name: 'SUITE', value: 'Relativity.ProcessingSetup')], propagate: false
	    }
	    
	    if (installing_analytics) {
            build job: 'test.Parameterized.Robot', parameters: [
    			[$class: 'NodeParameterValue', name: 'node_label', labels: [server_name], nodeEligibility: [$class: 'AllNodeEligibility']],
    			string(name: 'session_id', value: session_id),
    			string(name: 'SERVER', value: server_name),
    			string(name: 'DOMAIN', value: domain),
    			string(name: 'RelativityBuild', value: '"NULL"'),
    			string(name: 'RelativityType', value: '"NULL"'),
    			string(name: 'branch', value: 'tag_setup_tests'),
    			string(name: 'username', value: 'relativity.admin@kcura.com'),
    			string(name: 'SUITE', value: 'Relativity.AnalyticsSetup')], propagate: false
	    }
	}
	
	stage('Tests') {
        build_tests(server_name, domain, session_id, relativity_branch, automation_branch, installing_invariant, installing_datagrid)
    }
	
	passed = true
	status = "PASS"
}
finally {
    try {
		stage('Reporting') {
		    if (passed) {
        		build job: "Provision.VMware.DeleteVM", parameters: [
			        [$class: 'NodeParameterValue', name: 'node_label', labels: [server_name], nodeEligibility: [$class: 'AllNodeEligibility']],
			        string(name: 'vm_name', value: server_name)]
		    } else {
    			build job: 'Provision.Chef.DeleteChef',	parameters: [
    				[$class: 'NodeParameterValue', name: 'node_label', labels: [server_name + ' || chef'], nodeEligibility: [$class: 'AllNodeEligibility']],
    				string(name: 'node', value: random_server)]
    			
    			build job: "vCenter.shutdown_vms", parameters: [
    				[$class: 'NodeParameterValue', name: 'node_label', labels: [server_name + ' || chef'], nodeEligibility: [$class: 'AllNodeEligibility']],
    				string(name: 'VMs', value: "-v " + server_name)]
		    }
							
			build job: "Reporting.RegisterEvent", parameters: [
				[$class: 'NodeParameterValue', name: 'node_label', labels: [server_name], nodeEligibility: [$class: 'AllNodeEligibility']],
				string(name: 'action', value: '-ps'),
				string(name: 'session_id', value: session_id),
				string(name: 'status', value: status),
				string(name: 'deployment_step', value: 'Pipeline_Status'),
				string(name: 'host', value: random_server),
				string(name: 'event_hash', value: event_hash),
				string(name: 'job', value: env.BUILD_URL)]
							
			build job: "Reporting.AutomationReport", parameters: [
				[$class: 'NodeParameterValue', name: 'node_label', labels: [server_name], nodeEligibility: [$class: 'AllNodeEligibility']],
				string(name: 'branch', value: relativity_branch),
				string(name: 'session_id', value: session_id),
				string(name: 'slack_recipients', value: '#cd_poland'),
				string(name: 'notify', value: ''),
				string(name: 'event_hash', value: event_hash),
				string(name: 'report_health', value: 'report_health'),
				string(name: 'automation_branch', value: automation_branch)]
		}
	}
    finally {
	stage('Jenkins Slave Cleanup') {
		for (name_of_slave in names_of_slaves){
			build job: 'Provision.VMware.DeleteVM', parameters: [[$class: 'NodeParameterValue', name: 'node_label', labels: ['chef'], nodeEligibility: [$class: 'AllNodeEligibility']],
				string(name: 'vm_name', value: name_of_slave)],
				wait: false
			}
		}
	}
}

