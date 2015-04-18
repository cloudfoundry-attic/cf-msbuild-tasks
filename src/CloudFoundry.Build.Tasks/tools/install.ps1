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
			$buildProject.Xml.AddImport($Path)
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
	
	Add-Import "Properties\PublishProfiles\cf-push.pubxml" $project.Name

	Copy-Item "$toolsPath\cf-push.pubxml" $publishProfilePath -Force | Out-Null	

	Write-Host "Copying cf-push.pubxml file to project folder."
}

function Add-FileItemToProject($project, $toolsPath, $path, $file) {

	$ErrorActionPreference = "SilentlyContinue"
 
	# create folder path in project
	$pathParts = $path.split([System.IO.Path]::DirectorySeparatorChar);

	$currentItem = $project
	$newItem = $null
	for ($i = 0; $i -lt $pathParts.Length; $i++) {
		$newItem = $currentItem.ProjectItems.Item($pathParts[$i])
		if ($newItem -eq $null) {
			Write-Host "create folder " $pathParts[$i]
			$newItem = $currentItem.ProjectItems.AddFolder($pathParts[$i])			
		}
		if ($newItem -eq $null) {
			Write-Host "Error could not create folder for item " $pathParts[$i]
			return
		}
		$currentItem = $newItem
		$newItem = $null
	}
	$project.Save()

	# add file to project
	$itemFile = Join-Path $toolsPath $file

	$newItem = $currentItem.ProjectItems.Item($file)
	if ($newItem -eq $null) {
		$newItem = $currentItem.ProjectItems.AddFromFileCopy($itemFile)
		if ($newItem -eq $null) {
			Write-Host "Error could not copy file " $file
			return
		}
		# set 'Build Action' to 'None'
		$buildAction = $currentItem.Properties.Item("BuildAction")
		$buildAction.Value = 0
	}
	$project.Save()
	return
}

function UpdateDeployTargetFile($project, $xmlfile) {
	
	if ($project.DTE.Solution.IsOpen) {
		$dir = Split-Path $project.DTE.Solution.FileName -Parent
	}
	else {
		$dir = Split-Path $project.FileName -Parent
	}

	$tar = Get-ChildItem $dir -File "cf-msbuild-tasks.targets" -Recurse | Select-Object -First 1
	$rel = Resolve-Path $target.FullName -Relative

	$xml = [xml] (Get-Content $xmlfile)
	$xml.Project.PropertyGroup.DeployTargetFile = [string]$rel
	$xml.Save($xmlfile)
}

function Main 
{
	Copy-Item -Path (Join-Path $toolsPath "cf-push.pubxml") -Destination (Join-Path $toolsPath "profile.cf.pubxml") -Force | Out-Null
	UpdateDeployTargetFile $project (Join-Path $toolsPath "profile.cf.pubxml")
	Add-FileItemToProject $project $toolsPath "Properties\PublishProfiles" "profile.cf.pubxml"
}

Main
