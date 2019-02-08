. .\psake-common.ps1

properties {
    $where_expr = $tests_filter
    $is_quarantine = $is_quarantine
}

task default -depends test


task test_initalize {

    Write-Host "test task initialize " $integration_tests_filter

    If([System.IO.Directory]::Exists($testlog_directory)) {
        [System.IO.Directory]::Delete($testlog_directory, $true)
    }

    [System.IO.Directory]::CreateDirectory($testlog_directory)  
}

task test_reporting_initalize {

    Write-Host "test_reporting_initalize task initialize"

    If (-not [System.IO.Directory]::Exists($artifacts_directory))
    {
        [System.IO.Directory]::CreateDirectory($artifacts_directory)
    }

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

task get_reportunit -precondition { (-not [System.IO.File]::Exists($ReportUnit)) }  {
    $reportUnitVersion = '1.2.1'
    exec {
        & $nuget_exe @('install', 'ReportUnit', '-Version', $reportUnitVersion, '-ExcludeVersion')
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

function run_tests($test_type, $config_section, $where_string, $is_in_quarantine) {
    Write-Host "Is it run in Quarantine? " $is_in_quarantine

    if (-not [string]::IsNullOrEmpty($where_string)) {
        $where_string = '--where=' + $where_string
    }
    Write-Host $test_type " tests where expression: " $where_string

    $result_file = $config_section + "Results.xml"
    if ($is_in_quarantine) {
        $result_file = "Quarantine" + $result_file
    }
    Write-Host "Tests results file name: " $result_file

    exec {
        & $NUnit3 @($tests_project_file,
                    "--config=$config_section",
                    '--inprocess',
                    $where_string,
                    "--result=$result_file")
    }
}

task run_integration_tests -depends get_nunit {
    run_tests -test_type "Integration" -config_section "IntegrationTests" -where_string $where_expr -is_in_quarantine $is_quarantine
}

task run_ui_tests -depends get_nunit {
    run_tests -test_type "UI" -config_section "UITests" -where_string $where_expr -is_in_quarantine $is_quarantine
}

task generate_nunit_reports -depend get_reportunit, test_reporting_initalize {

    $result_files = "IntegrationTestsResults.xml","UITestsResults.xml","QuarantineIntegrationTestsResults.xml"
    foreach ($result_file in $result_files)
    {
        try {
            Write-Host "Generating html report for" $result_file
            Copy-Item $result_file -Destination $artifacts_directory 
            $report_file = $result_file -replace ".xml", ".html"
            exec {
                & $ReportUnit $result_file $report_file
            }
            Copy-Item $report_file -Destination $artifacts_directory 
        }
        catch {
            Write-Warning "Error occurred when generating nunit report $result_file"
        }
    }

}
