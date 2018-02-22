#!groovy

@Library('PipelineTools@develop')

def relativityBranch = env.relativityBranch ?: "develop"
def relativityBuildType = env.relativityBuildType ?: "DEV"
def relativityVersion = env.relativityVersion ?: ""

def skipBuild = env.skipBuild == "true"
def skipUnitTests = env.skipUnitTests == "true"
def skipProvisioning = env.skipProvisioning == "true"
def skipITests = env.skipITests == "true"

def ripBranch = (env.ripBranch ?: env.BRANCH_NAME) ?: "develop"
testsFilter = env.testsFilter ?: "cat == SmokeTest"

def installing_relativity = true
def installing_analytics = false
def installing_invariant = false
def installing_datagrid = false // TODO set to true

def run_list = createRunList(installing_relativity, installing_invariant, installing_analytics, installing_datagrid)
def profile = createProfile(installing_relativity, installing_invariant, installing_analytics, installing_datagrid)

def session_id = System.currentTimeMillis().toString()
def number_of_slaves = 2
def slackChannel = env.slackChannel ?: ('#cd_rip_' + ripBranch)
def event_hash = java.security.MessageDigest.getInstance("MD5").digest(env.JOB_NAME.bytes).encodeHex().toString()
def status = "FAIL"

def knife = 'C:\\Python27\\Lib\\site-packages\\jeeves\\knife.rb'
server_name = "DUMMY_SERVER_NAME"
domain = "DUMMY.COM"
ip = "DUMMY_IP"

has_errors = false

def NUnit = new nunit()

// Make changes here if necessary.
def chef_attributes = 'fluidOn:1,cdonprem:1'
def cookbook_versions = '"relativity:= 4.1.10,role-testvm:= 3.6.0,role-ci:= 1.2.0,analytics:= 4.0.0,invariant:= 4.1.2,datagrid:= 3.0.0,sql:= 2.2.4"'
def python_packages = 'CSharpLibrary==0.1.0 curiosity==3.0.0 fabric-venv==0.0.4 jeeves==4.1.0 jemin==1.0.0 kipa==2.0.0 kWebDriver==0.5 phonograph==5.0.0 RequestsLibrary==0.1.0 robotframework==3.0 robotframework-selenium2library==1.8.0 selenium==3.0.1 vmware==0.3.2'

// Unused
def analyticsbuildtype = "Unused"
def analyticsversion = "Unused"
def invariant_branch = "Unused"
def invariantbuild = "Unused"

println("""******************************************************
Configuration:

RIP branch: ${ripBranch}

Skip build: ${skipBuild}
Skip unit tests: ${skipUnitTests}
Skip VM provisioning: ${skipProvisioning}
Skip integration tests: ${skipITests}

Relativity branch: ${relativityBranch}
Relativity build type: ${relativityBuildType}
Relativity version: ${relativityVersion ?: "Relativity version was not set, newest one will be used."}

Install Analytics: ${installing_analytics}
Install Invariant: ${installing_invariant}
Install DataGrid: ${installing_datagrid}

Tests filter: ${testsFilter}

Session ID: ${session_id}
Number of dynamic slaves: ${number_of_slaves}
******************************************************""")

def getNUnitPath()
{
	def nunitConsolePath = "C:\\Program Files (x86)\\NUnit.org\\nunit-console\\nunit3-console.exe"
	if (!fileExists(nunitConsolePath))
	{
		nunitConsolePath = "C:\\Program Files (x86)\\NUnit.org\\3.6.1\\nunit-console\\nunit3-console.exe"
	}
	return nunitConsolePath
}

def modifyNUnitResultsForCuriosity(String session_id, String phase)
{
	//To provide compatibility with 'nunit_curiosity_listener' script we need to remove 'Properties' node from TestCase
	def commandString =  """Get-ChildItem .\\nunit-result.xml | %% {; 	[Xml]\$xml = Get-Content \$_.FullName;    \$xml | Select-Xml -XPath '//*[local-name() = ''properties'']' | ForEach-Object{\$_.Node.ParentNode.RemoveChild(\$_.Node)};    \$xml.OuterXml | Out-File .\\result.xml -encoding 'UTF8'; }"""
	bat String.format('powershell.exe "%s"', commandString)
			
	//And also ensure there is no Skipped test as 'nunit_curiosity_listener' doesn't handle 'Skipped; state
	commandString =  """Get-ChildItem .\\result.xml | %% {; 	[Xml]\$xml = Get-Content \$_.FullName; 	\$xml | Select-Xml -XPath '//*[@result = ''Skipped'']' | ForEach-Object{echo \$_.Node.LocalName; if(\$_.Node.LocalName -eq 'test-case'){\$_.Node.ParentNode.RemoveChild(\$_.Node)}}; \$xml.OuterXml | Out-File .\\result.xml -encoding 'UTF8'; }"""
	bat String.format('powershell.exe "%s"', commandString)
	
	//Finally we can run 'nunit_curiosity_listener' as we're almost certain there will be no problems
	bat String.format('python C:\\Python27\\Talos\\bin\\listeners\\nunit_curiosity_listener.py --phase %2$s -p .\\result.xml --session_id %1$s', session_id, phase)
	
	bat 'powershell.exe Remove-Item .\\nunit-result.xml'
}

def execute_nunit_tests(String test_dll, String session_id, String phase)
{
	try
	{
		def testsHavePassed = bat (script: """"${getNUnitPath()}" .\\lib\\UnitTests\\${test_dll} --inprocess --result=.\\nunit-result.xml;format=nunit2""", returnStatus: true) == 0
		has_errors = has_errors || !testsHavePassed
		println("Tests of ${test_dll} finished, has_errors = ${has_errors}")
	
		modifyNUnitResultsForCuriosity(session_id, phase)	
	}
	catch(Exception e)
	{
		println("Error in test block: " + e.toString())
		has_errors = true
	}
}

def execute_nunit_tests_2(String test_dll, String session_id, String phase)
{
	println("Running tests for ${test_dll} with filter ${testsFilter}, session_id=${session_id}")

	def where_clause = "_SKIPREASON !~ .+"
	if (testsFilter.trim() != '') {
		where_clause += ' && ' + testsFilter
	}
	
	try
	{
		withCredentials([usernamePassword(credentialsId: 'eddsdbo', passwordVariable: 'eddsdboPassword', usernameVariable: 'eddsdboUsername')])
		{
			def extra_args = """.\\lib\\UnitTests\\${test_dll} --where "${where_clause}" --inprocess --result=.\\nunit-result.xml;format=nunit2"""
			
			// TODO separate
			def nunitCommand = """python -m jeeves.create_config -t nunit -n "app.jeeves-ci" --dbuser "${eddsdboUsername}" --dbpass "${eddsdboPassword}" -s "${server_name}.kcura.corp" -db "${server_name}\\EDDSINSTANCE001" -o .\\lib\\UnitTests\\"""
			nunitCommand += """\n"${getNUnitPath()}" $extra_args"""
			println("NUnit command: " + nunitCommand)
			
			testsHavePassed = bat(script: nunitCommand, returnStatus: true) == 0
		}

		has_errors = has_errors || !testsHavePassed
		println "Tests of ${test_dll} finished, has_errors = ${has_errors}"
	
		modifyNUnitResultsForCuriosity(session_id, phase)
	} catch(Exception e) {
		println("Error in test block: " + e.toString())
		has_errors = true
	}
}

def build_tests(String server_name, String domain, String session_id, String relativityBranch, Boolean installing_invariant, Boolean installing_datagrid) {
	execute_nunit_tests_2("kCura.IntegrationPoints.Agent.Tests.Integration.dll", session_id, "production")
	execute_nunit_tests_2("kCura.IntegrationPoints.Core.Tests.Integration.dll", session_id, "production")
	execute_nunit_tests_2("kCura.IntegrationPoints.Data.Tests.Integration.dll", session_id, "production")
	execute_nunit_tests_2("kCura.IntegrationPoints.DocumentTransferProvider.Tests.Integration.dll", session_id, "production")
	execute_nunit_tests_2("kCura.IntegrationPoints.EventHandlers.Tests.Integration.dll", session_id, "production")
	execute_nunit_tests_2("kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.dll", session_id, "production")
	execute_nunit_tests_2("kCura.IntegrationPoints.ImportProvider.Tests.Integration.dll", session_id, "production")
	execute_nunit_tests_2("kCura.IntegrationPoints.Services.Tests.Integration.dll", session_id, "production")
	execute_nunit_tests_2("kCura.IntegrationPoints.Synchronizers.RDO.Tests.Integration.dll", session_id, "production")
	execute_nunit_tests_2("kCura.IntegrationPoints.Web.Tests.Integration.dll", session_id, "production")
	execute_nunit_tests_2("kCura.ScheduleQueue.Core.Tests.Integration.dll", session_id, "production")

	if (has_errors) {
		error("Tests failed.")
	}
}

timestamps
{
	timeout(time: 3, unit: 'HOURS')
	{
		catchError
		{
			parallel (
				BuildAndUnitTests:
				{
					node('buildslave')
					{			
						timeout(time: 20, unit: 'MINUTES')
						{
							stage('Build')
							{
								if (skipBuild) return;
								
								bat 'powershell.exe "taskkill /f /im msbuild.exe /T /fi \'IMAGENAME eq msbuild.exe\'"'

								bat 'subst W: /d || exit 0'
								bat 'echo "%WORKSPACE%"'
								bat 'subst W: "%WORKSPACE%"' 

								dir('W:/')
								{
									checkout([$class : 'GitSCM',
										branches : [[name : ripBranch]],
										doGenerateSubmoduleConfigurations : false,
										extensions :
										[[$class : 'CleanBeforeCheckout'],
											[$class : 'RelativeTargetDirectory',
												relativeTargetDir : 'integrationpoints']],
										submoduleCfg : [],
										userRemoteConfigs :
										[[credentialsId : 'TalosCI (bitbucket)',
											url : 'ssh://git@git.kcura.com:7999/in/integrationpoints.git']]]
									)
								}
								withCredentials([usernamePassword(credentialsId: 'talosci', passwordVariable: 'PaketPassword', usernameVariable: 'PaketUserName')])
								{
									dir('W:/integrationpoints')
									{
										bat 'powershell.exe "& {./build.ps1 release; exit $lastexitcode}"'
									}
								}
							}
						}
						
						timeout(time: 15, unit: 'MINUTES')
						{
							stage("Unit tests")
							{
								if (skipUnitTests) return;
								
								dir('W:/integrationpoints')
								{
									execute_nunit_tests("kCura.IntegrationPoints.Agent.Tests.dll", session_id, "staged")
									execute_nunit_tests("kCura.IntegrationPoints.Core.Contracts.Tests.dll", session_id, "staged")
									execute_nunit_tests("kCura.IntegrationPoints.Core.Tests.dll", session_id, "staged")
									execute_nunit_tests("kCura.IntegrationPoints.Data.Tests.dll", session_id, "staged")
									execute_nunit_tests("kCura.IntegrationPoints.DocumentTransferProvider.Tests.dll", session_id, "staged")
									execute_nunit_tests("kCura.IntegrationPoints.EventHandlers.Tests.dll", session_id, "staged")
									execute_nunit_tests("kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.dll", session_id, "staged")
									execute_nunit_tests("kCura.IntegrationPoints.FtpProvider.Helpers.Tests.dll", session_id, "staged")
									execute_nunit_tests("kCura.IntegrationPoints.FtpProvider.Parser.Tests.dll", session_id, "staged")
									execute_nunit_tests("kCura.IntegrationPoints.FtpProvider.Tests.dll", session_id, "staged")
									execute_nunit_tests("kCura.IntegrationPoints.ImportProvider.Parser.Tests.dll", session_id, "staged")
									execute_nunit_tests("kCura.IntegrationPoints.ImportProvider.Tests.dll", session_id, "staged")
									execute_nunit_tests("kCura.IntegrationPoints.Services.Tests.dll", session_id, "staged")
									execute_nunit_tests("kCura.IntegrationPoints.Synchronizers.RDO.Tests.dll", session_id, "staged")
									execute_nunit_tests("kCura.IntegrationPoints.Web.Tests.dll", session_id, "staged")
									execute_nunit_tests("kCura.ScheduleQueue.Core.Tests.dll", session_id, "staged")

									if (has_errors) {
										error("Tests failed.")
									}
								}
							}
						}

						timeout(time: 5, unit: 'MINUTES')
						{
							stage("Stash integration tests assemblies")
							{
								if (skipITests) return;

								dir('W:/integrationpoints')
								{
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
					}
				},
				InstallRAID:
				{	
					timeout(time: 50, unit: 'MINUTES')
					{
						stage('Install RAID')
						{							
							if (skipProvisioning) return;
							println("Getting server from pool, session_id: ${session_id}, Relativity branch: ${relativityBranch}, Relativity version: ${relativityVersion}, Relativity build type: ${relativityBuildType}, event hash: ${event_hash}")
							tuple = getServerFromPool(this, session_id, relativityBranch, relativityVersion, relativityBuildType, event_hash)							
							server_name = tuple[0]
							domain = tuple[1]
							ip = tuple[2]
							println("Acquired server: ${server_name}.${domain} (${ip})")

							parallel (
								Deploy:
								{
									registerEvent(this, session_id, 'Talos_Provision_test_CD', 'PASS', '-c', "${server_name}.${domain}", profile, event_hash)
									if(installing_relativity)
									{
										if (!relativityVersion)
										{
											relativityVersion = getBuildArtifactsPath(this, "Relativity", relativityBranch, relativityVersion, relativityBuildType, session_id)											
											println("Newest Relativity version found: " + relativityVersion)
										}
										println("Installing relativity, branch: ${relativityBranch}, version: ${relativityVersion}, type: ${relativityBuildType}")
										sendVersionToElastic(this, "Relativity", relativityBranch, relativityVersion, relativityBuildType, session_id)
									}
									
									if(installing_invariant)
									{
										invariantbuild = getBuildArtifactsPath(this, "Invariant", invariant_branch, invariantbuild, relativityBuildType, session_id)
										sendVersionToElastic(this, "Invariant", invariant_branch, invariantbuild, relativityBuildType, session_id)
									}
									
									uploadEnvironmentFile(this, server_name, relativityVersion, relativityBranch, relativityBuildType, invariantbuild, invariant_branch, cookbook_versions, chef_attributes, knife, analyticsversion, analyticsbuildtype, session_id, installing_relativity, installing_invariant, installing_analytics)
									withCredentials([
										usernamePassword(credentialsId: 'TestingAdministrator', passwordVariable: 'TAPassword', usernameVariable: 'TAUserName')])
									{
										addRunlist(this, session_id, server_name, domain, ip, run_list, knife, profile, event_hash, "testing\\$TAUserName", TAPassword)
									}
									checkTags(this, installing_relativity, installing_analytics, installing_invariant, installing_datagrid, server_name, knife, session_id, profile, event_hash)									
									checkWorkspaceUpgrade(this, server_name, session_id)
								},
								ProvisionNodes:
								{
									println("ML: provision nodes")
									createNodes(this, session_id, number_of_slaves)
									// TODO	try to delete, when everything works
									bootstrapDependencies(this, python_packages, relativityBranch, relativityVersion, relativityBuildType, session_id)
								}
							)
						}
					}
				}
			)
			
			stage("Unstash integration tests assemblies")
			{
				if (skipITests) return;
			
				timeout(time: 5, unit: 'MINUTES')
				{
					node("$session_id && dependencies")
					{						
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
					}
				}
			}

			stage('Integration Tests')
			{
				if (skipITests) return;

				timeout(time: 50, unit: 'MINUTES')
				{
					node("$session_id && dependencies")
					{					
						build_tests(server_name, domain, session_id, relativityBranch, installing_invariant, installing_datagrid)
					}
				}
			}
		}
		
		timeout(time: 5, unit: 'MINUTES')
		{
			stage('Reporting')
			{
				if (skipProvisioning) return;

				try
				{
					//NUnit.publish(this, session_id)

					if (has_errors)	status = "FAIL"

					node(session_id)
					{
						registerEvent(this, session_id, 'Pipeline_Status', status, '-ps', "$server_name.$domain", profile, event_hash, env.BUILD_URL)
						parallel([
							SlackNotification: { bat "python -m phonograph.slack_notify -f send_ci_status -s $session_id -r ${slackChannel}" },
							ChefCleanup: { bat "python -m jeeves.chef_functions -f delete_chef_artifacts -n $server_name -r '$knife'" }
						])
					}
				}
				catch (err)
				{
					echo err.toString()
				}
			}
		}

		timeout(time: 90, unit: 'MINUTES')
		{
			stage("Cleanup")
			{
				if (skipProvisioning) return;
				
				SaveVMs = false
				if (!currentBuild.result || currentBuild.result == "FAILURE")
				{
					try
					{
						timeout(time: 30, unit: 'MINUTES')
						{
							input(message: 'Save the VMs?', ok: 'Save', submitter: 'JNK-Basic')
						}
						SaveVMs = true
						saveVMs(this, session_id)
					}
					catch(err)
					{
						println("VMs won't be saved.")
					}
				}

				if (!SaveVMs)
				{
					deleteVMs(this, session_id)
				}
				deleteNodes(this, session_id)
			}
		}
	}
}
