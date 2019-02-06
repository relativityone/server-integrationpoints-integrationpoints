. .\psake-common.ps1


task default -depends nuget_publish

task nuget_publish {
    # TODO: Uncomment this once we're no longer building on TeamCity.
    # Assert($proget_api_key -ne $null -and $proget_api_key -ne "") 'proget_api_key must be provided to publish'

    $dictRef = @{}
    $dictPath = @{}
    $order = @()

    foreach ($o in (Get-ChildItem $nuspec_directory -File -Filter *.nupkg))
    {
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        $zip = [System.IO.Compression.ZipFile]::OpenRead($o.FullName)

        foreach ($item in $zip.Entries)
        {
            if ($item.FullName.EndsWith('.nuspec'))
            {
                $file = ([System.IO.StreamReader]$item.Open()).ReadToEnd()

                $xdoc = New-Object System.Xml.XmlDocument

                $xdoc.LoadXml($file)

                $id = $xdoc.package.metadata.id

                $dictRef.Add($id, @())
                $dictPath.Add($id, $o.FullName)

                foreach ($node in $xdoc.package.metadata.dependencies.dependency)
                {
                    $dictRef.Set_Item($id, ($dictRef.$id += $node.id))
                }
            }
        }

        $zip.Dispose()
    }

    while ($dictRef.Count -gt 0)
    {
        $key = ""

        foreach($ref in $dictRef.GetEnumerator())
        {
            $inOrder = $true

            foreach($dep in $ref.Value)
            {
                if ($dictRef.ContainsKey($dep))
                {
                    $inOrder = $false
                    break
                }
            }

            if ($inOrder)
            {
                $key = $ref.Key
                $order += $key
                break
            }
        }

        $dictRef.Remove($key)
    }

    foreach ($item in $order)
    {
        Write-Host "publishing" $dictPath.$item "..."

        exec {
            if ($proget_api_key)
            {
                & $nuget_exe @('push', $dictPath.$item, '-Source', $proget_server, '-ApiKey', $proget_api_key) 2>&1
            }
            else
            {
                # TODO: For use on TeamCity runs; remove once we're off TeamCity.
                & $nuget_exe @('push', $dictPath.$item, '-Source', $proget_server) 2>&1
            }
        }
    }
}
