. .\psake-common.ps1


task default -depends build_validation_message_table

task build_validation_message_table {
    $valDir = $source_directory + ".\kCura.IntegrationPoints.Core\Validation\"
    $xml = $valDir + "ValidationMessages.xml"
    $xsl = $valDir + "\ValidationMessages.xsl"
    $output = $development_scripts_directory + "\ValidationMessages.html"
    Write-Host $output
    $xslt = New-Object System.Xml.Xsl.XslCompiledTransform;
    $xslt.Load($xsl);
    $xslt.Transform($xml, $output);

    Write-Host "generated" $output;
}
