#TEMP till we have our own Azure subscription
$STORE_TEST_RESULT_URL = "https://testresultsanalyzer.azurewebsites.net/api/StoreTestResultFunction"

function store_tests_results($branch_id, $build_name, $test_type, $test_results_path, $securityCode) 
{
    $url = $STORE_TEST_RESULT_URL + "?code=" + $securityCode
    $rawTestResults = Get-Content $test_results_path -Raw
    $rawTestResults = format_to_xml_parsable_form $rawTestResults
	
    Write-Host "Storing test results in: $STORE_TEST_RESULT_URL"

    [xml]$xmlTestResults = $rawTestResults

    $storedCounter = 0
    $failedCounter = 0

    foreach ($testCase in $xmlTestResults.SelectNodes('//test-case')) 
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

        $request = Invoke-WebRequest -Uri $url -UseBasicParsing -ContentType "application/json" -Method POST -Body $body

        if($request.StatusCode -eq 200)
        {
            $storedCounter++
        }
        else
        {
            $failedCounter++
        }
    }

    Write-Host "Storing test results finished. Stored: $storedCounter Failed: $failedCounter"
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
    while ($node.type -ne 'TestSuite') 
	{
        $categories += $node.properties.property | where name -eq "Category" | select -ExpandProperty value
        $node = $node.ParentNode
    }
    $categories
}