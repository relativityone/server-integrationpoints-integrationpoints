$destinationDir = $env:LDAPSync + "\Dependencies\kCura"
$mainlineLibDir = $env:trunk + "\lib"

$dependencies = Get-ChildItem -Path $destinationDir

foreach($dependency in $dependencies)
{
    $items = Get-ChildItem -Path $mainlineLibDir -Filter $dependency -Recurse

    $fileExists = $items.Length -gt 0
    if ($fileExists) 
    {
        $name = $items[0].FullName
        Write-Host "Copying $name ..."
        Copy-Item -Path $items[0].FullName -Destination $destinationDir
    }
    else
    {
        Write-Host "Missing: $dependency" -ForegroundColor Red
    }
}

Write-Host "Update complete!"