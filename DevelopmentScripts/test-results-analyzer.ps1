#TEMP till we have our own Azure subscription
$STORE_TEST_RESULT_URL = "https://testresultsanalyzer.azurewebsites.net/api/StoreTestResultFunction"

function store_tests_results($branch_id, $build_name, $test_type, $test_results_path, $securityCode) 
{
    $url = "$STORE_TEST_RESULT_URL?code=$securityCode"
    $rawTestResults = Get-Content $test_results_path -Raw | format_to_xml_parsable_form

    [xml]$testResultsFile = $rawdata

    foreach ($testCase in $testResultsFile.SelectNodes('//test-case')) 
    {
        $body = @{
		    FullTestName = $testCase.fullname
		    Result = $testCase.result
		    BranchId = $branch_id
		    BuildName = $build_name
		    TestType = $test_type
		    Duration = $testCase.duration
		    Categories = @(find_categories $testCase)
		    Message = $testCase.output.'#cdata-section'
	    } | ConvertTo-Json

        Invoke-WebRequest -Uri $url -ContentType "application/json" -Method POST -Body $body
    }
}

function format_to_xml_parsable_form($rawcontent)
{
    get_rid_of_quotes_from_attribute_values $rawcontent
}

function get_rid_of_quotes_from_attribute_values($rawcontent)
{
    $matchesToReplace = Select-String '\(.*?(?<!\))\".*?\".*?\)' -input $rawcontent -AllMatches | Foreach {$_.matches.Value}

    foreach ($match in $matchesToReplace)
    {
        $replaced = $match -replace "`"", ""
        $rawcontent = $rawcontent -replace $match, $replaced
    }
    $rawcontent
}

function find_categories($testCaseNode)
{
    $categories = @()
    $node = $testCaseNode
    while ($node.type -ne 'TestSuite') {
        $categories += $node.properties.property | where name -eq "Category" | select -ExpandProperty value
        $node = $node.ParentNode
    }
    $categories
}