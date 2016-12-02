def passed = false
	try {
		
		node('buildslave') {

			stage('Checkout Integration Points') {
							
				dir('C:/SourceCode') {
					bat 'powershell.exe "taskkill /f /im msbuild.exe /T /fi \'IMAGENAME eq msbuild.exe\'"'

					checkout([$class : 'GitSCM',
							branches : [[name : env.BRANCH_NAME]],
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
			}

			stage('Build') {
				
				dir('C:/SourceCode/integrationpoints') {
					bat 'powershell.exe "& {./build.ps1 release; exit $lastexitcode}"'
				}			
			}
			
			stage('Unit Tests') {
				
				dir('C:/SourceCode/integrationpoints') {
					bat 'powershell.exe "& {./build.ps1 -test -skip; exit $lastexitcode}"'
				}			
			}
						
			dir('C:/SourceCode/integrationpoints') {
				stash includes: 'lib/UnitTests/*', name: 'testdlls'				
				stash includes: 'lib/UnitTests/TestData/*', name: 'testdata'				
				stash includes: 'lib/UnitTests/TestData/IMAGES/*', name: 'testdata_images'				
				stash includes: 'lib/UnitTests/TestData/NATIVES/*', name: 'testdata_natives'		
				stash includes: 'Applications/RelativityIntegrationPoints.Auto.rap', name: 'integrationPointsRap'		
			}
		}
		
		node('nunit') {

			stage('Integration Tests') {
				
				dir('C:/SourceCode/integrationpoints') {

					unstash 'testdlls'
					unstash 'testdata'
					unstash 'testdata_images'
					unstash 'testdata_natives'
					unstash 'integrationPointsRap'

					bat '"C:\\Program Files (x86)\\NUnit.org\\nunit-console\\nunit3-console.exe" lib\\UnitTests\\kCura.IntegrationPoints.Agent.Tests.Integration.dll lib\\UnitTests\\kCura.IntegrationPoints.Core.Tests.Integration.dll lib\\UnitTests\\kCura.IntegrationPoints.Data.Tests.Integration.dll lib\\UnitTests\\kCura.IntegrationPoints.DocumentTransferProvider.Tests.Integration.dll lib\\UnitTests\\kCura.IntegrationPoints.EventHandlers.Tests.Integration.dll lib\\UnitTests\\kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.dll lib\\UnitTests\\kCura.IntegrationPoints.Services.Tests.Integration.dll lib\\UnitTests\\kCura.IntegrationPoints.Synchronizers.RDO.Tests.Integration.dll lib\\UnitTests\\kCura.IntegrationPoints.Web.Tests.Integration.dll lib\\UnitTests\\kCura.ScheduleQueue.Core.Tests.Integration.dll --where "cat == SmokeTest" --result=C:\\SourceCode\\integrationpoints\\nunit-result.xml;format=nunit2'

					step([$class : 'XUnitBuilder',
							testTimeMargin : '3000',
							thresholdMode : 1,
							thresholds :
							[[$class : 'FailedThreshold',
									failureNewThreshold : '',
									failureThreshold : '',
									unstableNewThreshold : '',
									unstableThreshold : ''],
								[$class : 'SkippedThreshold',
									failureNewThreshold : '',
									failureThreshold : '',
									unstableNewThreshold : '',
									unstableThreshold : '']],
							tools :
							[[$class : 'NUnitJunitHudsonTestType',
									deleteOutputFiles : true,
									failIfNotNew : true,
									pattern : 'nunit-result.xml',
									skipNoTestFiles : false,
									stopProcessingIfError : true]]])
				}
			}
		}

		node('robot') {

			stage('Checkout Automation') {

				dir('C:/SourceCode') {
				
					checkout([$class : 'GitSCM',
							branches : [[name : 'develop']],
							doGenerateSubmoduleConfigurations : false,
							extensions :
							[[$class : 'CleanBeforeCheckout'],
								[$class : 'RelativeTargetDirectory',
									relativeTargetDir : 'automation']],
							submoduleCfg : [],
							userRemoteConfigs :
							[[credentialsId : 'TalosCI (bitbucket)',
								url : 'ssh://git@git.kcura.com:7999/aut/automation.git']]])

				}					
			}

			stage('Functional Smoke Tests') {

				dir('C:/SourceCode/automation/') {
				
					bat 'kBot.exe --log "C:\\SourceCode\\automation\\log.html" --report "C:\\SourceCode\\automation\\RIP_upgrade_report.html" --outputdir "C:\\SourceCode\\automation" --argumentfile "C:\\SourceCode\\automation\\Config\\pl2.cfg" -s "Automation.PostInstall.InstallApps.UpdateRIPApp" "C:\\SourceCode\\automation"'
					
					bat 'kBot.exe --log "C:\\SourceCode\\automation\\log.html" --report "C:\\SourceCode\\automation\\report.html" --outputdir "C:\\SourceCode\\automation" --argumentfile "C:\\SourceCode\\automation\\Config\\pl2.cfg" -s "Tests.Relativity.Applications.RelativityIntegrationPoints.SmokeTests" "C:\\SourceCode\\automation"'
				}			
			}

			step([$class : 'RobotPublisher',
					disableArchiveOutput : false,
					logFileName : 'log.html',
					otherFiles : '',
					outputFileName : 'output.xml',
					outputPath : 'C:\\SourceCode\\automation',
					passThreshold : 100,
					reportFileName : 'report.html',
					unstableThreshold : 0]);				
		}
		
		passed = true
	}

	finally {
		node{
			def to = emailextrecipients([
				[$class: 'DevelopersRecipientProvider'],
				[$class: 'RequesterRecipientProvider']
			])
			
			if (!passed) {
				/*mail body: 'https://tt-jnk-poland.testing.corp/view/All/job/IntegrationPoint_IntegrationTests', subject: 'Integration Points Integration Tests FAILED', to: 'bmollus@kcura.com,lstarzyk@kcura.com'*/
				emailext body: 'https://tt-jnk-poland.testing.corp/job/IntegrationPoints/', recipientProviders: [[$class: 'DevelopersRecipientProvider'], [$class: 'RequesterRecipientProvider']], subject: 'Integration Points Pipeline FAILED', to: 'lstarzyk@kcura.com'
			}
			if (passed) {
				/*mail body: 'https://tt-jnk-poland.testing.corp/view/All/job/IntegrationPoint_IntegrationTests', subject: 'Integration Points Integration Tests PASSED', to: 'bmollus@kcura.com,lstarzyk@kcura.com'*/
				emailext body: 'https://tt-jnk-poland.testing.corp/job/IntegrationPoints/', recipientProviders: [[$class: 'DevelopersRecipientProvider'], [$class: 'RequesterRecipientProvider']], subject: 'Integration Points Pipeline PASSED', to: 'lstarzyk@kcura.com'
			}
		}
	}
