. .\psake-common.ps1

properties {
    $in_where_expr = $integration_tests_filter
    $ui_where_expr = $ui_tests_filter
}

$reportUnitVersion = '1.2.1'

task default -depends test


task test_initalize {

    Write-Host "test task initialize " $integration_tests_filter

    If([System.IO.Directory]::Exists($testlog_directory)) {
        [System.IO.Directory]::Delete($testlog_directory, $true)
    }

    [System.IO.Directory]::CreateDirectory($testlog_directory)  
}

task get_testrunner -precondition { (-not [System.IO.File]::Exists($testrunner_exe)) }  {
    exec {
        & $nuget_exe @('install', 'kCura.TestRunner', '-ExcludeVersion')
    }    
    Copy-Item ([System.IO.Path]::Combine($development_scripts_directory, 'kCura.TestRunner', 'lib', 'kCura.TestRunner.exe')) $development_scripts_directory
}

task get_nunit -precondition { (-not [System.IO.File]::Exists($NUnit3)) }  {
    exec {
        & $nuget_exe @('install', 'NUnit.Console', '-Version', '3.4.1', '-ExcludeVersion')
    } 
}

task test -depends get_testrunner, get_nunit, test_initalize {
    exec {
        & $testrunner_exe @(('/source:' + $root), 
                            ('/tests:' + $inputfile), 
                            ('/out:' + $testlog_directory), 
                            ('/nunit3:' + $NUnit3),
                            ('/timeout:' + 5),
                            ('/timeoutWarning:' + 3))
    }
}

task run_integration_tests -depends get_nunit {
    if (-not [string]::IsNullOrEmpty($in_where_expr)) {
        $in_where_expr = '--where=' + $in_where_expr
    }
    Write-Host "Integration tests where expression: " $in_where_expr
    exec {
        & $NUnit3 @($tests_project_file,
                    '--config="IntegrationTests"',
                    '--inprocess',
                    $in_where_expr,
                    '--result="IntegrationTestsResults.xml"')
    }
}

task generate_integration_tests_report {
    exec {
        & $nuget_exe @('install', 'ReportUnit', '-Version', $reportUnitVersion, '-ExcludeVersion')
        & ./ReportUnit/tools/reportunit "IntegrationTestsResults.xml" "IntegrationTestsResults.html"
    } 
}

task generate_quarantined_integration_tests_report {
    exec {
        & $nuget_exe @('install', 'ReportUnit', '-Version', $reportUnitVersion, '-ExcludeVersion')
        & ./ReportUnit/tools/reportunit "QuarantinedIntegrationTestsResults.xml" "QuarantinedIntegrationTestsResults.html"
    } 
}

task run_ui_tests -depends get_nunit {
    if (-not [string]::IsNullOrEmpty($ui_where_expr)) {
        $ui_where_expr = '--where=' + $ui_where_expr
    }
    Write-Host "UI tests where expression: " $ui_where_expr
    exec {
        & $NUnit3 @($tests_project_file,
                    '--config="UITests"',
                    '--inprocess',
                    $ui_where_expr,
                    '--result="UITestsResults.xml"')
    }
}

task generate_ui_tests_report {
    exec {
        & $nuget_exe @('install', 'ReportUnit', '-Version', $reportUnitVersion, '-ExcludeVersion')
        & ./ReportUnit/tools/reportunit "UITestsResults.xml" "UITestsResults.html"
    } 
}
