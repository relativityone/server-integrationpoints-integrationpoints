$destinationDir = "C:\SourceCode\IntegrationPoints\source\Dependencies\kCura"
$mainlineLibDir = "C:\SourceCode\Mainline\lib"

$dependencies = Get-ChildItem -Path $destinationDir

foreach($dependency in $dependencies)
{
    $fileExists = Test-Path "$mainlineLibDir\$dependency"
    if ($fileExists) 
    {
        Write-Host "Updating $dependency.Name ..."
        Copy-Item -Path "$mainlineLibDir\$dependency" -Destination $destinationDir
    }
}

Write-Host "Update complete!"