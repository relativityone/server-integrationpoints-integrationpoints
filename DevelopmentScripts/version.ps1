. .\common.ps1


task default -depends version


task version {

    $NewVersion = 'AssemblyVersionAttribute("' + $version + '")'
    $NewFileVersion = 'AssemblyFileVersionAttribute("' + $version + '")'
    $NewCopyright = 'AssemblyCopyrightAttribute("Copyright (c) ' + [System.DateTime]::Now.Year + ', ' + $Company + '")'
    $NewTitle = 'AssemblyTitleAttribute("' + $Product + '")'
    $NewDescription = 'AssemblyDescriptionAttribute("' + $ProductDescription + '")'
    $NewCompany = 'AssemblyCompanyAttribute("' + $Company + '")'
    $NewProduct = 'AssemblyProductAttribute("' + $Product + '")'

    foreach($o in Get-ChildItem $version_directory){

       if($o.BaseName -ne 'AssemblyInfo') {continue}
       
       Write-Host "Updating" $o.FullName "to version" $version "..."
       
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



