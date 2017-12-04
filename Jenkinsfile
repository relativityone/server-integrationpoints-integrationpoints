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
installing_analytics = 'false'
installing_invariant = 'false'
installing_datagrid = 'false'
knife = "C:\\Python27\\Lib\\site-packages\\jeeves\\knife.rb"
username = "testing\\Administrator"
password = "P@ssw0rd@1"
is_nightly_test_execution_develop = false
has_errors = false


try {	
	is_nightly_test_execution_develop = params.nightly_test_execution_develop
}
catch (MissingPropertyException e) {
	//parameter 'params.nightly_test_execution_develop' is not set in other branches than nightly.
}

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
    run_list = "role-ci::install_relativity,role-ci::install_analytics,role-ci::install_invariant,role-ci::update_config,license::default,role-ci::configure_analytics,role-ci::invariantEndpoint,role-ci::install_elastic,role-ci::upgrade_datagrid_sql"
} else {
    if (installing_relativity) {
        profile += "_rel"
        run_list += ",role-ci::install_relativity"
    }
    if (installing_analytics) {
        profile += "_ana"
        run_list += ",role-ci::install_analytics"
    }
    if (installing_invariant) {
        profile += "_inv"
        run_list += ",role-ci::install_invariant"
    }
    if (installing_relativity) {
        run_list += ",role-ci::update_config,license::default"
    }
    if (installing_analytics) {
        run_list += ",role-ci::configure_analytics"
    }
    if (installing_invariant) {
        run_list += ",role-ci::invariantEndpoint"
    }
    if (installing_datagrid) {
        profile += "_dg"
        run_list += ",role-ci::install_elastic,role-ci::upgrade_datagrid_sql"
    }
    run_list = run_list[1..-1]
}

def relativity_branch = 'develop'
def automation_branch = 'develop'
def invariant_branch = 'develop'

def passed = false
def status = "FAIL"
def ip = ""
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
  
  
  
def get_ip(name) {
   def ugly = String.format('python -m vmware.create_ci_environment --platform get_ip_address_of_vm -s %1$s', name)
   def ip = bat returnStdout: true, script: ugly
   return ip.trim().split('\r\n')[2]
}

def send_slack_message(recipients_list, message, color="E8E8E8") {
    for (recipient in recipients_list) {
        slackSend channel: "${recipient}", color: "${color}", message: "${message}", teamDomain: 'kcura-pd', token: 'nd9zSTCysTjKA7Ky5mMCCA5b'
        sleep(1) // Slack has a rate limit of 1 msg / second
    }
}

  
stage('Get Server') {
    def file_name = UUID.randomUUID().toString() + ".txt"
	def windows_path = "\\\\kcura.corp\\sdlc\\Testing\\TestingData\\PooledServers\\" + file_name
	def linux_path = "/mnt/kcura.corp/sdlc/testing/TestingData/PooledServers/" + file_name

	build job: 'Provision.VMware.GetServerFromPool', parameters: [
        [$class: 'NodeParameterValue', name: 'node_label', labels: ['chef'], nodeEligibility: [$class: 'AllNodeEligibility']],
        string(name: 'temp_file', value: windows_path),
        string(name: 'pool_name', value: 'cook')]
					
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
     
    node ('chef') {
        ip = get_ip(server_name)
        echo ip
    }
}

def execute_nunit_tests(String test_dll, String tests_filter, String session_id, String phase){

	def where_clause = "_SKIPREASON !~ .+"
	
	if(tests_filter != ''){
		where_clause = where_clause + ' && ' + tests_filter
	}
	
	dir('c:/SourceCode/integrationpoints'){
		try {		
			def nunitPath1 = "C:\\Program Files (x86)\\NUnit.org\\nunit-console\\nunit3-console.exe"
			def nunitPath2 = "C:\\Program Files (x86)\\NUnit.org\\3.6.1\\nunit-console\\nunit3-console.exe"			
			def nunitConsolePath = ''
			
			if (fileExists(nunitPath1))
			{
				nunitConsolePath = nunitPath1
			}
			else
			{
				nunitConsolePath = nunitPath2
			}

		
			def testsHavePassed = bat (script: String.format('"'+nunitConsolePath+'" C:\\SourceCode\\integrationpoints\\lib\\UnitTests\\%1$s --where "%2$s" --inprocess --result=C:\\SourceCode\\integrationpoints\\nunit-result.xml;format=nunit2', "$test_dll", "$where_clause"), 
			returnStatus: true) == 0
						
			has_errors = has_errors || !testsHavePassed			
			println 'DEBUG: has_errors = ' + has_errors
		
			//To provide compatibility with 'nunit_curiosity_listener' script we need to remove 'Properties' node from TestCase
			def commandString =  """Get-ChildItem .\\nunit-result.xml | %% {; 	[Xml]\$xml = Get-Content \$_.FullName;    \$xml | Select-Xml -XPath '//*[local-name() = ''properties'']' | ForEach-Object{\$_.Node.ParentNode.RemoveChild(\$_.Node)};    \$xml.OuterXml | Out-File .\\result.xml -encoding 'UTF8'; }"""
			bat String.format('powershell.exe "%s"', commandString)
					
			//And also ensure there is no Skipped test as 'nunit_curiosity_listener' doesn't handle 'Skipped; state
			commandString =  """Get-ChildItem .\\result.xml | %% {; 	[Xml]\$xml = Get-Content \$_.FullName; 	\$xml | Select-Xml -XPath '//*[@result = ''Skipped'']' | ForEach-Object{echo \$_.Node.LocalName; if(\$_.Node.LocalName -eq 'test-case'){\$_.Node.ParentNode.RemoveChild(\$_.Node)}}; \$xml.OuterXml | Out-File .\\result.xml -encoding 'UTF8'; }"""
					bat String.format('powershell.exe "%s"', commandString)
			
			//Finally we can run 'nunit_curiosity_listener' as we're almost certain there will be no problems
			bat String.format('python C:\\Python27\\Talos\\bin\\listeners\\nunit_curiosity_listener.py --phase %2$s -p C:\\SourceCode\\integrationpoints\\result.xml --session_id %1$s', session_id, phase)
			
			bat 'powershell.exe Remove-Item C:\\SourceCode\\integrationpoints\\nunit-result.xml'
		
		} catch(Exception e){  		
			has_errors = true
		}
	}
}

// WTF Java has a 64kb limit for any function that it runs.
// This means that the number of steps that we can run is limited.
// https://issues.jenkins-ci.org/browse/JENKINS-37984
def build_tests(String server_name, String domain, String session_id, String relativity_branch, String automation_branch, Boolean installing_invariant, Boolean installing_datagrid, Boolean is_nightly_test_execution_develop) {
		def tests_filter = "cat == SmokeTest"
		if (is_nightly_test_execution_develop) {
			tests_filter = ""
		}
		
		//Integration tests
		execute_nunit_tests("kCura.IntegrationPoints.Agent.Tests.Integration.dll", tests_filter, session_id, "production")
		execute_nunit_tests("kCura.IntegrationPoints.Core.Tests.Integration.dll", tests_filter, session_id, "production")
		execute_nunit_tests("kCura.IntegrationPoints.Data.Tests.Integration.dll", tests_filter, session_id, "production")
		execute_nunit_tests("kCura.IntegrationPoints.DocumentTransferProvider.Tests.Integration.dll", tests_filter, session_id, "production")
		execute_nunit_tests("kCura.IntegrationPoints.EventHandlers.Tests.Integration.dll", tests_filter, session_id, "production")
		execute_nunit_tests("kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.dll", tests_filter, session_id, "production")
		execute_nunit_tests("kCura.IntegrationPoints.ImportProvider.Tests.Integration.dll", tests_filter, session_id, "production")
		execute_nunit_tests("kCura.IntegrationPoints.Services.Tests.Integration.dll", tests_filter, session_id, "production")
		execute_nunit_tests("kCura.IntegrationPoints.Synchronizers.RDO.Tests.Integration.dll", tests_filter, session_id, "production")
		execute_nunit_tests("kCura.IntegrationPoints.Web.Tests.Integration.dll", tests_filter, session_id, "production")
		execute_nunit_tests("kCura.ScheduleQueue.Core.Tests.Integration.dll", tests_filter, session_id, "production")
		
		//RIP_System_Tests: {build job: 'test.Parameterized.Robot', parameters: [
    	//	[$class: 'NodeParameterValue', name: 'node_label', labels: [server_name], nodeEligibility: [$class: 'AllNodeEligibility']],
    	//	string(name: 'session_id', value: session_id),
    	//	string(name: 'SERVER', value: server_name),
    	//	string(name: 'DOMAIN', value: domain),
    	//	string(name: 'RelativityBuild', value: '"NULL"'),
    	//	string(name: 'RelativityType', value: '"NULL"'),
    	//	string(name: 'branch', value: automation_branch),
    	//	string(name: 'config_file', value: 'Config/smokeTests.cfg'),
    	//	string(name: 'SUITE', value: 'Relativity.Applications.RelativityIntegrationPoints.SmokeTests')]}
}

try {

	node('buildslave') {
		stage('IP Build & UnitTests') {
						
			dir('C:/SourceCode') {
				bat 'powershell.exe "taskkill /f /im msbuild.exe /T /fi \'IMAGENAME eq msbuild.exe\'"'

				def rip_branch = env.BRANCH_NAME
				if (is_nightly_test_execution_develop) {
					rip_branch = '*/develop'
				}				
				
				checkout([$class : 'GitSCM',
				branches : [[name : rip_branch]],
				doGenerateSubmoduleConfigurations : false,
				extensions :
				[[$class : 'CleanBeforeCheckout'],
					[$class : 'RelativeTargetDirectory',
						relativeTargetDir : 'integrationpoints']],
				submoduleCfg : [],
				userRemoteConfigs :
				[[credentialsId : 'TalosCI (bitbucket)',
					url : 'ssh://git@git.kcura.com:7999/in/integrationpoints.git']]])
			}		
			
			dir('C:/SourceCode/integrationpoints') {
				bat 'powershell.exe "& {./build.ps1 release; exit $lastexitcode}"'
			}			
			
			//Unit tests
			execute_nunit_tests("kCura.IntegrationPoints.Agent.Tests.dll", "", session_id, "staged")
			execute_nunit_tests("kCura.IntegrationPoints.Core.Contracts.Tests.dll", "", session_id, "staged")
			execute_nunit_tests("kCura.IntegrationPoints.Core.Tests.dll", "", session_id, "staged")
			execute_nunit_tests("kCura.IntegrationPoints.Data.Tests.dll", "", session_id, "staged")
			execute_nunit_tests("kCura.IntegrationPoints.DocumentTransferProvider.Tests.dll", "", session_id, "staged")
			execute_nunit_tests("kCura.IntegrationPoints.EventHandlers.Tests.dll", "", session_id, "staged")
			execute_nunit_tests("kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.dll", "", session_id, "staged")
			execute_nunit_tests("kCura.IntegrationPoints.FtpProvider.Helpers.Tests.dll", "", session_id, "staged")
			execute_nunit_tests("kCura.IntegrationPoints.FtpProvider.Parser.Tests.dll", "", session_id, "staged")
			execute_nunit_tests("kCura.IntegrationPoints.FtpProvider.Tests.dll", "", session_id, "staged")
			execute_nunit_tests("kCura.IntegrationPoints.ImportProvider.Parser.Tests.dll", "", session_id, "staged")
			execute_nunit_tests("kCura.IntegrationPoints.ImportProvider.Tests.dll", "", session_id, "staged")
			execute_nunit_tests("kCura.IntegrationPoints.Services.Tests.dll", "", session_id, "staged")
			execute_nunit_tests("kCura.IntegrationPoints.Synchronizers.RDO.Tests.dll", "", session_id, "staged")
			execute_nunit_tests("kCura.IntegrationPoints.Web.Tests.dll", "", session_id, "staged")
			execute_nunit_tests("kCura.ScheduleQueue.Core.Tests.dll", "", session_id, "staged")
			

			dir('C:/SourceCode/integrationpoints') {
				stash includes: 'lib/UnitTests/*', name: 'testdlls'				
				stash includes: 'lib/UnitTests/TestData/*', name: 'testdata'				
				stash includes: 'lib/UnitTests/TestData/IMAGES/*', name: 'testdata_images'				
				stash includes: 'lib/UnitTests/TestData/NATIVES/*', name: 'testdata_natives'		
				stash includes: 'lib/UnitTests/oi/*', name: 'outside_in'		
				stash includes: 'lib/UnitTests/TestDataForImport/*', name: 'testdataforimport'
				stash includes: 'lib/UnitTests/TestDataForImport/et-files/*', name: 'testdataforimport_et'
				stash includes: 'lib/UnitTests/TestDataForImport/img/*', name: 'testdataforimport_img'
				stash includes: 'lib/UnitTests/TestDataForImport/native-files/*', name: 'testdataforimport_native'
				stash includes: 'DynamicallyLoadedDLLs/Search-Standard/*', name: 'DynamicallyLoadedDLLs'
				stash includes: 'Applications/RelativityIntegrationPoints.Auto.rap', name: 'integrationPointsRap'		
			}
		}

	}

    stage('Install RAID') {
    	parallel Deploy: {
    		build job: "Reporting.RegisterEvent", parameters: [
                [$class: 'NodeParameterValue', name: 'node_label', labels: ['chef'], nodeEligibility: [$class: 'AllNodeEligibility']],
                string(name: 'action', value: '-c'),
                string(name: 'session_id', value: session_id),
                string(name: 'deployment_step', value: 'Talos_Provision_test_CD'),
                string(name: 'host', value: random_server),
                string(name: 'profile', value: profile),
                string(name: 'event_hash', value: event_hash),
                string(name: 'exclude_verification_steps', value: 'true')]
    						
    		build job: 'Provision.Chef.UploadEnvironmentFile', parameters: [
				string(name: 'type', value: type),
				string(name: 'build', value: build),
				string(name: 'branch', value: relativity_branch),
				string(name: 'invariantbuild', value: invariantbuild),
				string(name: 'invariantbranch', value: invariant_branch),
				string(name: 'template', value: "V2"),
				string(name: 'server', value: server_name),
				string(name: 'knife', value: "$knife"),
				string(name: 'session_id', value: session_id),
				string(name: 'username', value: username),
				string(name: 'password', value: password),
				[$class: 'NodeParameterValue', name: 'node_label', labels: [server_name + ' || chef'], nodeEligibility: [$class: 'AllNodeEligibility']]]
    			
    		node ('chef') {
			try {
				bat String.format('knife node run_list add %1$s %3$s -c %2$s', server_name, "$knife", run_list)
				bat String.format('python -m jeeves.register_event --session_id %1$s -ds chef_setup --status PASS -u --host %2$s --profile %3$s --event_hash %4$s --component relativity --job_link unknown', session_id, random_server, profile, event_hash)
				} catch(Exception ex)  {
					has_errors = true
					bat String.format('python -m jeeves.register_event --session_id %1$s -ds chef_setup --status FAIL -u --host %2$s --profile %3$s --event_hash %4$s --component relativity --job_link unknown', session_id, random_server, profile, event_hash)
					}
				try {
				  bat String.format('python -m jeeves.chef_functions -f run_chef_client -n %1$s -r %2$s -un %3$s -up %4$s', ip, "$knife", "$username", "$password")
				}catch(Exception ex)  {
					has_errors = true
					echo 'Something failed in the installation'
				}
			}
    		
    		if (installing_relativity) {
				build job: 'Provision.Chef.WaitForTags', parameters: [
					string(name: 'node', value: server_name),
					string(name: 'tags', value: '-t relativityInstalled'),
					string(name: 'session_id', value: session_id),
					string(name: 'deployment_step', value: 'Relativity_Installation'),
					string(name: 'component', value: 'relativity'),
					string(name: 'timeout', value: '300'),
					string(name: 'knife', value: knife),
					[$class: 'NodeParameterValue', name: 'node_label', labels: [server_name], nodeEligibility: [$class: 'AllNodeEligibility']]]
			}
			 
			if (installing_invariant) {
				build job: 'Provision.Chef.WaitForTags', parameters: [
					string(name: 'node', value: server_name),
					string(name: 'tags', value: '-t invariantInstalled'),
					string(name: 'session_id', value: session_id),
					string(name: 'deployment_step', value: 'Invariant_Installation'),
					string(name: 'component', value: 'invariant'),
					string(name: 'timeout', value: '300'),
					string(name: 'knife', value: knife),
					[$class: 'NodeParameterValue', name: 'node_label', labels: [server_name], nodeEligibility: [$class: 'AllNodeEligibility']]]
			}
		 
			if (installing_analytics) {
				build job: 'Provision.Chef.WaitForTags', parameters: [
					string(name: 'node', value: server_name),
					string(name: 'tags', value: '-t analyticsInstalled'),
					string(name: 'session_id', value: session_id),
					string(name: 'deployment_step', value: 'Analytics_Installation'),
					string(name: 'component', value: 'analytics'),
					string(name: 'timeout', value: '300'),
					string(name: 'knife', value: knife),
					[$class: 'NodeParameterValue', name: 'node_label', labels: [server_name], nodeEligibility: [$class: 'AllNodeEligibility']]]
			}
		 
			if (installing_datagrid) {
				build job: 'Provision.Chef.WaitForTags', parameters: [
					string(name: 'node', value: server_name),
					string(name: 'tags', value: '-t datagridInstalled'),
					string(name: 'session_id', value: session_id),
					string(name: 'deployment_step', value: 'DataGrid_Installation'),
					string(name: 'component', value: 'datagrid'),
					string(name: 'timeout', value: '300'),
					string(name: 'knife', value: knife),
					[$class: 'NodeParameterValue', name: 'node_label', labels: [server_name], nodeEligibility: [$class: 'AllNodeEligibility']]]
			}
			node(server_name)
			{
				def database = server_name + "\\EDDSINSTANCE001"
			   bat String.format('python -m jeeves.validation -f check_workspace_upgrade -e %1$s', database)
			}
		 
		}, ProvisionNodes: {
			build job: "Provision.VMware.ExpiryAttribute", parameters: [
				[$class: 'NodeParameterValue', name: 'node_label', labels: ['chef'], nodeEligibility: [$class: 'AllNodeEligibility']],
				string(name: 'node', value: random_server),
				string(name: 'hours', value: '12')]
				 
			build job: "vCenter.create_jenkins_slaves", parameters: [
			[$class: 'NodeParameterValue', name: 'node_label', labels: ['chef'], nodeEligibility: [$class: 'AllNodeEligibility']],
				string(name: 'TemplateName', value: 'TT-Win7-IE10-Robot3'),
				string(name: 'VMNames', value: names_of_slaves.toString()),
				string(name: 'StartSequence', value: '1'),
				string(name: 'NumberOfSlaves', value: number_of_slaves),
				string(name: 'JenkinsServer', value: 'POLAND'),
				string(name: 'Label', value: server_name)]
		 
			for (name_of_slave in names_of_slaves){
				build job: 'test.Nodes.PullTestDependencies', parameters: [
					[$class: 'NodeParameterValue', name: 'node_label', labels: [name_of_slave], nodeEligibility: [$class: 'AllNodeEligibility']],
					string(name: 'RelativityBuild', value: build),
					string(name: 'RelativityType', value: type),
					string(name: 'branch', value: relativity_branch),
					string(name: 'automation', value: automation_branch)],
					wait: false
			}
		}, failFast: true
    }

	node(server_name) {
		stage('Tests') {
			bat 'mkdir "C:\\SourceCode\\integrationpoints"'
			def cmd = "powershell.exe setx JenkinsBuildHost " + random_server + " /m"
			bat cmd

			dir('C:/SourceCode/integrationpoints') {
				unstash 'testdlls'
				unstash 'testdata'
				unstash 'testdata_images'
				unstash 'testdata_natives'
				unstash 'integrationPointsRap'
				unstash 'outside_in'
				unstash 'testdataforimport'
				unstash 'testdataforimport_et'
				unstash 'testdataforimport_img'
				unstash 'testdataforimport_native'				
				unstash 'DynamicallyLoadedDLLs'

				build_tests(server_name, domain, session_id, relativity_branch, automation_branch, installing_invariant, installing_datagrid, is_nightly_test_execution_develop)
			}	
		}
	}	
	
	passed = true
	if (!has_errors)
		status = "PASS"
	
	println 'DEBUG: has_errors: ' + has_errors + ' , status: ' + status
}
finally {
    try {
		stage('Reporting') {
		    if (passed) {
				build job: "Provision.VMware.DeleteVM", parameters: [
					[$class: 'NodeParameterValue', name: 'node_label', labels: ['chef'], nodeEligibility: [$class: 'AllNodeEligibility']],
					string(name: 'vm_name', value: server_name),
					string(name: 'knife', value: knife)]
			} else {
    			
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
				[$class: 'NodeParameterValue', name: 'node_label', labels: ['chef'], nodeEligibility: [$class: 'AllNodeEligibility']],
				string(name: 'branch', value: relativity_branch),
				string(name: 'session_id', value: session_id),
				string(name: 'slack_recipients', value: '#cd_poland'),
				string(name: 'notify', value: ''),
				string(name: 'event_hash', value: event_hash),
				string(name: 'report_health', value: 'report_health'),
				string(name: 'exclude_post_install_steps', value: 'true')]
				
			

			// Customized Jenkins Slack notification:
			if (is_nightly_test_execution_develop)
			{
				def slackMessage = "\n *Commits:*"
				for (changelog in currentBuild.changeSets)
				{
					for (commit in changelog.items)
					{
						slackMessage += "\n\n*${commit.author}:* ${commit.msg}"
					}
				}			
				
				def message = "*${status}*: <${env.BUILD_URL}|Build #${env.BUILD_NUMBER}> from <${env.JOB_URL}|${env.JOB_NAME}> \n${slackMessage}"
				if (has_errors)
					message = "@here --> " + message
				
				send_slack_message(['#cd_poland'], message, (has_errors) ? 'ff0000' : '00ff00')	
			}
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
	
	//this part is crucial to return the error. Otherwise Jenkins returns SUCCESS.
	if (has_errors)
		error "The build has errors."
}