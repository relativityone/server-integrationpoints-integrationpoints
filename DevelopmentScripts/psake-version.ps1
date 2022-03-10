. .\psake-common.ps1


task default -depends version


task version {
    $NewVersion = 'AssemblyVersion("' + $version + '")'
    $NewFileVersion = 'AssemblyFileVersion("' + $version + '")'

    $buildtypeversioninfo = $build_type

    
    if($build_config -eq 'Debug') {
        $buildtypeversioninfo += '-Debug'
    }

    if($server_type -eq 'local') {
        $buildtypeversioninfo += '-local'
    }


    $NewInfoVersion = 'AssemblyInformationalVersion("' + $version + '-' + $branch + '-' + $buildtypeversioninfo + '")'
    $NewCopyright = 'AssemblyCopyright("Copyright (c) ' + [System.DateTime]::Now.Year + ', ' + $company + '")'
    $NewTitle = 'AssemblyTitle("' + $product + '")'
    $NewDescription = 'AssemblyDescription("' + $product_description + '")'
    $NewCompany = 'AssemblyCompany("' + $company + '")'
    $NewProduct = 'AssemblyProduct("' + $product + '")'

    foreach ($o in (Get-ChildItem $version_directory -File -Filter AssemblyInfo.*))
    {
       Write-Host "Updating" $o.FullName "to version" $version "..."
       
       $tmp = Get-Content $o.FullName | 
       %{$_ -replace 'AssemblyVersion\(".*"\)', $NewVersion} |
       %{$_ -replace 'AssemblyFileVersion\(".*"\)', $NewFileVersion} |
       %{$_ -replace 'AssemblyInformationalVersion\(".*"\)', $NewInfoVersion} |
       %{$_ -replace 'AssemblyCopyright\(".*"\)', $NewCopyright} |
       %{$_ -replace 'AssemblyTitle\(".*"\)', $NewTitle} |
       %{$_ -replace 'AssemblyDescription\(".*"\)', $NewDescription} |
       %{$_ -replace 'AssemblyCompany\(".*"\)', $NewCompany} |
       %{$_ -replace 'AssemblyProduct\(".*"\)', $NewProduct}

       [System.IO.File]::WriteAllLines($o.FullName, $tmp)
    }   
}
