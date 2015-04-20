param($installPath, $toolsPath, $package, $project)

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

	$target = Get-ChildItem $dir -File "cf-msbuild-tasks.targets" -Recurse | Select-Object -First 1
	$rel = Resolve-Path $target.FullName -Relative

	$xml = [xml] (Get-Content $xmlfile)
	$xml.Project.PropertyGroup.DeployTargetFile = [string]$rel
	$xml.Save($xmlfile)
}

function Main 
{
	UpdateDeployTargetFile $project (Join-Path $toolsPath "push.cf.pubxml")
	Add-FileItemToProject $project $toolsPath "Properties\PublishProfiles" "push.cf.pubxml"
}

Main
