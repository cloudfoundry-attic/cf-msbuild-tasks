param($installPath, $toolsPath, $package, $project)

function Resolve-ProjectName {
    param(
        [parameter(ValueFromPipelineByPropertyName = $true)]
        [string[]]$ProjectName
    )
    
    if($ProjectName) {
        $projects = Get-Project $ProjectName
    }
    else {
        # All projects by default
        $projects = Get-Project
    }
    
    $projects
}

function Get-MSBuildProject {
    param(
        [parameter(ValueFromPipelineByPropertyName = $true)]
        [string[]]$ProjectName
    )
    Process {
        (Resolve-ProjectName $ProjectName) | % {
            $path = $_.FullName
            @([Microsoft.Build.Evaluation.ProjectCollection]::GlobalProjectCollection.GetLoadedProjects($path))[0]
            
        }
    }
}

function Add-Import {
    param(
        [parameter(Position = 0, Mandatory = $true)]
        [string]$Path,
        [parameter(Position = 1, ValueFromPipelineByPropertyName = $true)]
        [string[]]$ProjectName
    )
    Process {
        
        (Resolve-ProjectName $ProjectName) | %{
            $buildProject = $_ | Get-MSBuildProject
            $importElement = $buildProject.Xml.AddImport($Path)
			$importElement.Condition = "Exists(`'${path}`')"
            $_.Save()
        }
    }
}

function Add-ItemNoneInclude {
    param(
        [parameter(Position = 0, Mandatory = $true)]
        [string]$Path,
        [parameter(Position = 1, ValueFromPipelineByPropertyName = $true)]
        [string[]]$ProjectName
    )
    Process {
        
        (Resolve-ProjectName $ProjectName) | %{
            $buildProject = $_ | Get-MSBuildProject
            $buildProject.Xml.AddItem("None", $Path)
            $_.Save()
        }
    }
}

function Copy-Resources($project) {
	$publishProfilePath = (Join-Path $project.Name "Properties\PublishProfiles")

	# Create PublishProfiles folder in project Properties folder.
	if(!(Test-Path $publishProfilePath)) {
		mkdir $publishProfilePath | Out-Null
	}
	
	# Copy cf publish profile to Proprties location
	Copy-Item "$toolsPath\cf-push.pubxml" $publishProfilePath -Force | Out-Null	

	$publishProfile = "Properties\PublishProfiles\cf-push.pubxml"

	# Add Import Project cf publish profile to .csproj destination file
	Add-Import $publishProfile $project.Name

	# Add Item None Include cf publish profile to .csproj destination file
	Add-ItemNoneInclude $publishProfile $project.Name
}

function Main 
{
	Copy-Resources $project
}

Main
