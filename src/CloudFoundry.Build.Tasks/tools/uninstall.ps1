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

function Remove-Resources($project) {
	$projectName = $project.Name
	#Remove-Item "$projectName\Properties\PublishProfiles\cf-push.pubxml" -Force | Out-Null
	#Remove-Import "Properties\PublishProfiles\cf-push.pubxml" $project.Name
}

function Main 
{
	Remove-Resources $project
}

Main
