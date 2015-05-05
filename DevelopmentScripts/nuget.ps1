. .\common.ps1


task default -depends update_nuspec, nuget_pack


task update_nuspec {
    foreach($o in Get-ChildItem $nuspec_directory){
       
       if($o.Extension -ne '.nuspec') {continue}

       Write-Host "Updating" $o.FullName "to version" $version "..."
       
       $x = Select-Xml -Path $o.FullName -XPath '/package/metadata/version'
       $x.Node.InnerText = $version   
       $x.Node.OwnerDocument.Save($x.Path)   
       
       $x = Select-Xml -Path $o.FullName -XPath '/package/metadata/copyright'
       $x.Node.InnerText = "Copyright (c) " + [System.DateTime]::Now.Year + ", " + $Company  
       $x.Node.OwnerDocument.Save($x.Path)  
       
       $x = Select-Xml -Path $o.FullName -XPath '/package/metadata/authors'
       $x.Node.InnerText = $Company  
       $x.Node.OwnerDocument.Save($x.Path)   
       
       $x = Select-Xml -Path $o.FullName -XPath '/package/metadata/owners'
       $x.Node.InnerText = $Company  
       $x.Node.OwnerDocument.Save($x.Path)       
    }   
}

task nuget_pack {
     foreach($o in Get-ChildItem $nuspec_directory){
        
        if($o.Extension -ne '.nuspec') {continue}

        Write-Host "Packing" $o.FullName "..."

        exec {
            & $nuget_exe @('pack', $o.FullName)
        }
     }    
}
