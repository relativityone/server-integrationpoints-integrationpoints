. .\common.ps1


task default -depends version


task version {
    foreach($o in Get-ChildItem $version_directory){
       
       Write-Host "Updating" $o.FullName "to version" $version
       
       $tmp = Get-Content $o.FullName | 
       %{$_ -replace 'AssemblyVersionAttribute\(".*"\)', $NewVersion} |
       %{$_ -replace 'AssemblyFileVersionAttribute\(".*"\)', $NewFileVersion} |
       %{$_ -replace 'AssemblyCopyrightAttribute\(".*"\)', $NewCopyright} |
       %{$_ -replace 'AssemblyTitleAttribute\(".*"\)', $NewTitle} |
       %{$_ -replace 'AssemblyDescriptionAttribute\(".*"\)', $NewDescription} |
       %{$_ -replace 'AssemblyCompanyAttribute\(".*"\)', $NewCompany} |
       %{$_ -replace 'AssemblyProductAttribute\(".*"\)', $NewProduct}

       [System.IO.File]::WriteAllLines($o.FullName, $tmp)
    }   
}



