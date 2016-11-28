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
			}
		}
		
		node('nunit') {

			stage('Integration Tests') {
				
				dir('C:/SourceCode/integrationpoints') {

					unstash 'testdlls'
					unstash 'testdata'
					unstash 'testdata_images'
					unstash 'testdata_natives'

					bat '"C:\\Program Files (x86)\\NUnit.org\\nunit-console\\nunit3-console.exe" lib\\UnitTests\\kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.dll --test=kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Process.ExportProcessRunnerTest.RunTestCase,kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Process.ExportProcessRunnerTest.RunInvalidFileshareTestCase --result=C:\\SourceCode\\integrationpoints\\nunit-result.xml;format=nunit2'

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
		if (passed) {
			mail body : env.BUILD_URL,
			subject : 'Integration Points Pipeline PASSED',
			to : 'bmollus@kcura.com'
		} else {
			mail body : env.BUILD_URL,
			subject : 'Integration Points Pipeline FAILED',
			to : 'bmollus@kcura.com'
		}
	}
