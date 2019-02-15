#TEMP till we have our own Azure subscription
$STORE_TEST_RESULT_URL = "https://testresultsanalyzer.azurewebsites.net/api/StoreTestResultFunction?code=BqgMKu1Mp/WMNPKfacvIGR4oSzBpDGgdxKNGYnAOOPrwe9DHYQidlA=="

function store_tests_results($branch_id, $build_name, $test_type, $test_results_path) 
{
    $rawTestResults = Get-Content $test_results_path -Raw | formatToXmlParsableForm

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
		    Categories = @(findCategories $testCase)
		    Message = $testCase.output.'#cdata-section'
	    } | ConvertTo-Json

        Invoke-WebRequest -Uri $STORE_TEST_RESULT_URL -ContentType "application/json" -Method POST -Body $body
    }
}

function formatToXmlParsableForm($rawcontent)
{
    $matchesToReplace = Select-String '\(.*?(?<!\))\".*?\".*?\)' -input $rawcontent -AllMatches | Foreach {$_.matches.Value}

    foreach ($match in $matchesToReplace)
    {
        $replaced = $match -replace "`"", ""
        $rawcontent = $rawcontent -replace $match, $replaced
    }
    $rawcontent
}

function findCategories($testCaseNode)
{
    $categories = @()
    $node = $testCaseNode
    while ($node.type -ne 'TestSuite') {
        $categories += $node.properties.property | where name -eq "Category" | select -ExpandProperty value
        $node = $node.ParentNode
    }
    $categories
}