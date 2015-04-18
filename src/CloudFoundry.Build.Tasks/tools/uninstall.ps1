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

function Remove-Import {
    param(
        [parameter(Position = 0, Mandatory = $true)]
        [string]$Name,
        [parameter(Position = 1, ValueFromPipelineByPropertyName = $true)]
        [string[]]$ProjectName
    )
    Process {
        (Resolve-ProjectName $ProjectName) | %{
            $buildProject = $_ | Get-MSBuildProject
			$importToRemove = $buildProject.Xml.Imports | Where-Object { $_.Project.Endswith($Name) }
			$buildProject.Xml.RemoveChild($importToRemove) | out-null
            $_.Save()
        }
    }
}

function Remove-ItemNoneInclude {
    param(
        [parameter(Position = 0, Mandatory = $true)]
        [string]$Name,
        [parameter(Position = 1, ValueFromPipelineByPropertyName = $true)]
        [string[]]$ProjectName
    )
    Process {
        (Resolve-ProjectName $ProjectName) | %{
            $buildProject = $_ | Get-MSBuildProject
			$itemToRemove = $buildProject.Xml.Items | Where-Object { $_.Include.Endswith($Name) }
			$itemToRemove.Parent.RemoveChild($itemToRemove) | out-null
            $_.Save()
        }
    }
}

function Remove-Resources($project) {
	$projectName = $project.Name

	# Remove cf publish profile from Proprties location
	Remove-Item "$projectName\Properties\PublishProfiles\cf.pushxml" -Force | Out-Null

	$publishProfile = "Properties\PublishProfiles\cf.pushxml"
	# Remove Import Project cf publish profile from .csproj destination file
	Remove-Import $publishProfile $project.Name

	# Remove Item None Include cf publish profile from .csproj destination file
	Remove-ItemNoneInclude $publishProfile $project.Name
}

function Main 
{
	Remove-Resources $project
}

Main
